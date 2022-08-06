using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectColorPickerHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject _colorPicker;
    private bool _colorPickerActive;

    private Camera cam;

    private GestureDetection gestDetect;
    private bool colorGestDone = false;

    private InteractionType interaction;

    void Start()
    {
        interaction = Utilities._interaction;

        cam = Camera.main;

        if (interaction == InteractionType.Kinect)
        {
            gestDetect = GameObject.Find("GestureDetectHandler").GetComponent<GestureDetection>();

            if (gestDetect != null)
            {
                gestDetect.OnGesture += ListenForColorGesture;
            }
        }
    }

    void Update()
    {
        Transform theObj = cam.gameObject.GetComponent<SelectObject>().GetSelectedObject();
        if (theObj)
        {
            if (!WhiteboardHandler._whiteboardActive)
            {
                bool colorTrigger = (interaction == InteractionType.Mouse) ?
                    Input.GetKeyDown(KeyCode.I) :
                    colorGestDone;

                if (colorTrigger)
                {
                    colorGestDone = false;
                    _colorPickerActive = !_colorPickerActive;
                    _colorPicker.GetComponent<Image>().gameObject.SetActive(_colorPickerActive);

                    if (!_colorPickerActive)
                    {
                        Color col = Camera.main.GetComponent<SelectObject>().GetObjectColor();
                        Utilities.ChangeObjectColor(col);
                    }
                }
            }
        }
        else
        {
            if (_colorPickerActive)
            {
                CloseColorPicker();
            }
        }
    }

    private void CloseColorPicker()
    {
        _colorPickerActive = false;
        _colorPicker.GetComponent<Image>().gameObject.SetActive(_colorPickerActive);
    }


    private void ListenForColorGesture(GestureDetection.EventArgs e)
    {
        if (e.name.Contains("Two"))
        {
            if (e.confidence > Utilities.TwoFingersThreshold && !WhiteboardHandler._whiteboardActive &&
                cam.gameObject.GetComponent<SelectObject>().GetSelectedObject())
            {
                // Debug.Log("Color picker triggered");
                colorGestDone = true;
                gestDetect.OnGesture -= ListenForColorGesture;
                StartCoroutine(ReactivateColorGesture());
            }
        }
    }

    private IEnumerator ReactivateColorGesture()
    {
        yield return new WaitForSeconds(2);
        gestDetect.OnGesture += ListenForColorGesture;
    }
}
