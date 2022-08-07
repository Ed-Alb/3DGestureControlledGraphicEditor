using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Kinect.VisualGestureBuilder;
using Windows.Kinect;
using System;

public class GestureDetection : MonoBehaviour
{
    public struct EventArgs
    {
        public string name;
        public float confidence;

        public EventArgs(string _name, float _confidence)
        {
            name = _name;
            confidence = _confidence;
        }
    }

    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

#if !UNITY_EDITOR
    private string gbdpath = "\\PointingGesture.gbd";
#else
    private string gbdpath = "\\GesturesDB\\PointingGesture.gbd";
#endif

    private VisualGestureBuilderDatabase _gesturesDatabase;

    [SerializeField] private BodySourceManager _BodySource;

    private KinectSensor _Sensor;

    private VisualGestureBuilderFrameSource _gestureFrameSource;
    private VisualGestureBuilderFrameReader _gestureFrameReader;

    public delegate void GestureAction(EventArgs e);
    public event GestureAction OnGesture;

    private List<Gesture> gests = new List<Gesture>();

    [SerializeField] private Text DebT;

    private void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _gestureFrameSource = VisualGestureBuilderFrameSource.Create(_Sensor, 0);

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }

            _gestureFrameReader = _gestureFrameSource.OpenReader();

            if (_gestureFrameReader != null)
            {
                _gestureFrameReader.IsPaused = true;
                _gestureFrameReader.FrameArrived += GestureFrameArrived;
            }

#if UNITY_EDITOR
            _gesturesDatabase = VisualGestureBuilderDatabase.Create(desktopPath + gbdpath);
#else
            _gesturesDatabase = VisualGestureBuilderDatabase.Create(Application.streamingAssetsPath + gbdpath);
#endif
            Gesture[] gestureArray;
            using (VisualGestureBuilderDatabase database = _gesturesDatabase)
            {
                List<Gesture> ges = new List<Gesture>(_gesturesDatabase.AvailableGestures);
                gestureArray = ges.ToArray();
            }

            _gestureFrameSource.AddGestures(gestureArray);
        }
    }

    private void Update()
    {
        // Cand detecteaza mainile ar trebui sa se opreasca
        if (!_gestureFrameSource.IsTrackingIdValid)
        {
            FindValidBody();
        }
    }

    private void FindValidBody()
    {
        if (_BodySource != null)
        {
            Body[] bodies = _BodySource.GetData();
            if (bodies != null)
            {
                foreach (Body body in bodies)
                {
                    if (body.IsTracked)
                    {
                        SetBody(body.TrackingId);
                        break;
                    }
                }
            }
        }
    }

    public void SetBody(ulong id)
    {
        if (id > 0)
        {
            _gestureFrameSource.TrackingId = id;
            _gestureFrameReader.IsPaused = false;
        }
        else
        {
            _gestureFrameSource.TrackingId = 0;
            _gestureFrameReader.IsPaused = true;
        }
    }

    private void GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
    {
        VisualGestureBuilderFrameReference frameReference = e.FrameReference;

        using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
        {
            if (frame != null)
            {
                IDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;
                //DebT.text = "SUNT Acolo";
                if (discreteResults != null)
                {
                    foreach (Gesture gesture in _gestureFrameSource.Gestures)
                    {
                        if (gesture.GestureType == GestureType.Discrete)
                        {
                            DiscreteGestureResult result = null;
                            discreteResults.TryGetValue(gesture, out result);
                            if (result != null)
                            {
                                OnGesture(new EventArgs(gesture.Name, result.Confidence));
                            }
                        }
                    }
                }
            }
        }
    }
}
