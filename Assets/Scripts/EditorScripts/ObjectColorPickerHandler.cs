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

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Transform theObj = cam.gameObject.GetComponent<SelectObject>().GetSelectedObject();
        if (theObj)
        {
            //Debug.Log(theObj.gameObject.name);
            if (!WhiteboardHandler._whiteboardActive)
            {
                if (Input.GetKeyDown(KeyCode.I))
                {
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
}
