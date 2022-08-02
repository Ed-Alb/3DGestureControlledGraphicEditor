using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private string databasePathLeft = "\\GesturesDatabase\\LeftAction_Left.gba";
    private string databasePathRight = "\\GesturesDatabase\\RightAction_Right.gba";
    private string databasePathClose = "\\GesturesDatabase\\CloseAction.gba";

    public BodySourceManager _BodySource;

    private KinectSensor _Sensor;

    private VisualGestureBuilderDatabase _gestureDatabaseLeftHand;
    private VisualGestureBuilderDatabase _gestureDatabaseRightHand;
    private VisualGestureBuilderDatabase _gestureDatabaseClose;
    private VisualGestureBuilderFrameSource _gestureFrameSource;
    private VisualGestureBuilderFrameReader _gestureFrameReader;

    public delegate void GestureAction(EventArgs e);
    public event GestureAction OnGesture;

    private void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        _gestureDatabaseLeftHand = VisualGestureBuilderDatabase.Create(desktopPath + databasePathLeft);
        _gestureDatabaseRightHand = VisualGestureBuilderDatabase.Create(desktopPath + databasePathRight);
        _gestureDatabaseClose = VisualGestureBuilderDatabase.Create(desktopPath + databasePathClose);
        
        _gestureFrameSource = VisualGestureBuilderFrameSource.Create(_Sensor, 0);
        _gestureFrameReader = _gestureFrameSource.OpenReader();

        if (_gestureFrameReader != null)
        {
            _gestureFrameReader.IsPaused = true;
            _gestureFrameReader.FrameArrived += GestureFrameArrived;
        }

        if (_gestureDatabaseLeftHand != null)
        {
            foreach (Gesture g in _gestureDatabaseLeftHand.AvailableGestures)
            {
                //Debug.Log(g.Name);
                _gestureFrameSource.AddGesture(g);
            }
        }

        if (_gestureDatabaseRightHand != null)
        {
            foreach (Gesture g in _gestureDatabaseRightHand.AvailableGestures)
            {
                //Debug.Log(g.Name);
                _gestureFrameSource.AddGesture(g);
            }
        }

        if (_gestureDatabaseClose != null)
        {
            foreach (Gesture g in _gestureDatabaseClose.AvailableGestures)
            {
                //Debug.Log(g.Name);
                _gestureFrameSource.AddGesture(g);
            }
        }
    }

    private void Update()
    {
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
