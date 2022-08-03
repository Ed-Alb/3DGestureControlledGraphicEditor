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

    void Start()
    {
        cam = Camera.main;
        gestDetect = GameObject.Find("GestureDetectHandler").GetComponent<GestureDetection>();
        if (gestDetect != null)
        {
            gestDetect.OnGesture += ListenForColorGesture;
        }
    }

    private void ListenForColorGesture(GestureDetection.EventArgs e)
    {
        if (e.name.Contains("Two"))
        {
            if (e.confidence > .1f && !WhiteboardHandler._whiteboardActive &&
                cam.gameObject.GetComponent<SelectObject>().GetSelectedObject())
            {
                // Debug.Log("Color picker triggered");
                colorGestDone = true;
                gestDetect.OnGesture -= ListenForColorGesture;
                StartCoroutine(ReactivateColorGesture());
            }
        }
    }

    void Update()
    {
        Transform theObj = cam.gameObject.GetComponent<SelectObject>().GetSelectedObject();
        if (theObj)
        {
            //Debug.Log(theObj.gameObject.name);
            if (!WhiteboardHandler._whiteboardActive)
            {
                if (Input.GetKeyDown(KeyCode.I) || colorGestDone)
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

    private IEnumerator ReactivateColorGesture()
    {
        yield return new WaitForSeconds(2);
        gestDetect.OnGesture += ListenForColorGesture;
    }
}
