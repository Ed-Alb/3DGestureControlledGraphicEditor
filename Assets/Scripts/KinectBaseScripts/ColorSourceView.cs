using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;
using System;

public class ColorSourceView : MonoBehaviour
{
    public GameObject ColorSourceManager;
    private ColorSourceManager _ColorManager;
    
    void Start ()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }
    
    void Update()
    {
        KinectSensor _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            if (_Sensor.IsAvailable)
            {
                if (ColorSourceManager == null)
                {
                    return;
                }

                _ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
                if (_ColorManager == null)
                {
                    return;
                }

                var rend = GetComponent<MeshRenderer>();
                rend.enabled = true;
                gameObject.GetComponent<Renderer>().material.mainTexture = _ColorManager.GetColorTexture();
            }
            else
            {
                var rend = GetComponent<MeshRenderer>();
                rend.enabled = false;
            }
        }
    }
}
