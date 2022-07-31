using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhiteboardHandler : MonoBehaviour
{
    public static bool _whiteboardActive = false;
    public GameObject WhiteboardPrefab;
    public GameObject WhiteboardSpawnPoint;
    private GameObject WhiteboardInstantiated;

    private Vector3 lastCamPos;
    private Quaternion lastCamRot;

    private bool firstTime = true;

    [SerializeField]
    private GameObject PenPanel;

    [SerializeField]
    private GameObject _penPanelImage;

    [SerializeField] private GameObject _colorPicker;
    private bool _colorPickerActive = false;
    private bool _hideMarkerOnExit = false;

    [SerializeField] private GameObject _marker;

    [SerializeField] private Renderer _markerTipRenderer;

    // Update is called once per frame
    void Update()
    {
        Camera cam = Camera.main;

        if (_whiteboardActive)
        {
            TriggerWhiteboardColorPicker();
        }
        else if (!_whiteboardActive && _hideMarkerOnExit && _colorPickerActive)
        {
            ExitWhiteboard();
        }

        if (Input.GetKeyDown(KeyCode.T) && !cam.transform.gameObject.GetComponent<SelectObject>().GetSelectedObject())
        {
            _whiteboardActive = !_whiteboardActive;

            if (!_whiteboardActive)
            {
                ActivateAllButtons(true);
                cam.transform.position = lastCamPos;
                cam.transform.rotation = lastCamRot;
                ViewWhiteboard(false);
                PenPanel.SetActive(false);
            }
            else if (_whiteboardActive)
            {
                ActivateAllButtons(false);
                lastCamPos = cam.transform.position;
                lastCamRot= cam.transform.rotation;

                GameObject.Find("ObjectsPanel").GetComponent<SpawnObject>().HideObjectsName();

                if (firstTime)
                {
                    WhiteboardInstantiated = Instantiate(WhiteboardPrefab, WhiteboardSpawnPoint.transform);
                    firstTime = false;
                }
                else
                {
                    ViewWhiteboard(true);
                }

                PenPanel.SetActive(true);
                _marker.GetComponent<Marker>().RenderMarker(true);

                Transform _cameraTransform = WhiteboardInstantiated.transform.Find("CameraPos");
                cam.transform.position = _cameraTransform.position;
                cam.transform.rotation = _cameraTransform.rotation;
            }
        }
    }

    private void ActivateAllButtons(bool state)
    {
        ActivateSpawnButtons(state);
        ActivateObjectsButtons(state);
    }

    public void ActivateSpawnButtons(bool state)
    {
        GameObject ObjectsPanel = GameObject.Find("ObjectsPanel");
        foreach (Transform child_button in ObjectsPanel.transform)
        {
            child_button.GetComponent<Button>().interactable = state;
        }
    }

    public void ActivateObjectsButtons(bool state)
    {
        // Disable all the buttons on the left as well
        GameObject ObjectsListContent = GameObject.Find("ObjectsListContent");
        foreach (Transform child_button in ObjectsListContent.transform)
        {
            child_button.GetComponent<Button>().interactable = state;
        }
    }

    private void TriggerWhiteboardColorPicker()
    {
        _hideMarkerOnExit = true;

        float penImageScale = _marker.GetComponent<Marker>().getPenImageScale();
        AdjustPenSizeImage(penImageScale);

        // Trigger apparition of ColorPicker while using the whiteboard
        if (Input.GetKeyDown(KeyCode.C))
        {
            _colorPickerActive = !_colorPickerActive;
            _colorPicker.GetComponent<Image>().gameObject.SetActive(_colorPickerActive);

            if (!_colorPickerActive)
            {
                _marker.GetComponent<Marker>().activateMarker(true);

                Utilities.ChangeMarkerColor(_markerTipRenderer.material.color, _markerTipRenderer, _penPanelImage);
            }
        }
    }

    private void AdjustPenSizeImage(float penImageScale)
    {
        _penPanelImage.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(penImageScale, penImageScale);
    }

    private void ExitWhiteboard()
    {
        _colorPickerActive = false;
        _hideMarkerOnExit = false;
        _colorPicker.GetComponent<Image>().gameObject.SetActive(_colorPickerActive);
        //Debug.Log("Marker Hid");
    }


    //     Iterate though each child of the Whiteboard and 
    // disable/enable the Renderer so that when you open it
    // again, the drawings will remain
    public void ViewWhiteboard(bool state)
    {
        WhiteboardInstantiated.GetComponent<Renderer>().enabled = state;
        foreach (Transform component in WhiteboardInstantiated.transform)
        {
            if (!component.gameObject.name.Equals("CameraPos"))
            {
                component.gameObject.GetComponent<Renderer>().enabled = state;
            }
        }
    }
}
