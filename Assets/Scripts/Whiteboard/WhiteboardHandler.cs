using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WhiteboardType { Sketch, Terrain };

public class WhiteboardHandler : MonoBehaviour
{
    public static bool _whiteboardActive = false;

    public GameObject WhiteboardPrefab;
    public GameObject TerrainWhiteboardPrefab;

    public GameObject WhiteboardSpawnPoint;
    public GameObject TerrainWhiteboardSpawnPoint;

    private GameObject WhiteboardInstantiated;
    private GameObject TerrainWhiteboardInstantiated;

    private Vector3 lastCamPos;
    private Quaternion lastCamRot;

    private bool firstTime = true;
    private bool firstTimeTerrain = true;
    public static bool SketchMode = false;
    public static bool TerrainMode = false;

    [SerializeField]
    private GameObject PenPanel;

    [SerializeField]
    private GameObject _penPanelImage;

    [SerializeField] private GameObject _colorPicker;
    [SerializeField] private GameObject _terrainColorPicker;
    private bool _colorPickerActive = false;
    private bool _terrainColorPickerActive = false;
    private bool _hideMarkerOnExit = false;

    [SerializeField] private GameObject _marker;

    [SerializeField] private Renderer _markerTipRenderer;

    private GestureDetection gestDetect;
    private bool whiteboardGest = false, terrWhiteGest = false, colorGest = false;

    private InteractionType _interaction;
    private WhiteboardType whiteboardType = WhiteboardType.Sketch;

    private void Start()
    {
        _interaction = Utilities._interaction;

        if (_interaction == InteractionType.Kinect)
        {
            gestDetect = GameObject.Find("GestureDetectHandler").GetComponent<GestureDetection>();
            if (gestDetect != null)
            {
                gestDetect.OnGesture += ListenForWhiteboardActivateGesture;
                gestDetect.OnGesture += ListenForTerrainWhiteboardActivateGesture;
                gestDetect.OnGesture += ListenForColorGesture;
            }
        }
    }

    private void ListenForWhiteboardActivateGesture(GestureDetection.EventArgs e)
    {
        // if I did the gesture that activated the whiteboard, then
        // I stop listening to the event and wait for 2 seconds if I
        // want to do the gesture again, so that I doesn't switch
        // between whiteboard and normal view forever
        if (e.name.Contains("ActWhiteboard"))
        {
            if (e.confidence > Utilities.WhiteboardThreshold && !whiteboardGest && !TerrainMode &&
                !Camera.main.transform.gameObject.GetComponent<SelectObject>().GetSelectedObject())
            {
                // Debug.Log("Whiteboard triggered");
                whiteboardGest = true;
                gestDetect.OnGesture -= ListenForWhiteboardActivateGesture;
                StartCoroutine(ReactivateWhiteGesture());
            }
        }
    }
    
    private void ListenForTerrainWhiteboardActivateGesture(GestureDetection.EventArgs e)
    {
        if (e.name.Contains("Terrain"))
        {
            if (e.confidence > Utilities.WhiteboardThreshold && !terrWhiteGest && !SketchMode &&
                !Camera.main.transform.gameObject.GetComponent<SelectObject>().GetSelectedObject())
            {
                // Debug.Log("Whiteboard triggered");
                terrWhiteGest = true;
                gestDetect.OnGesture -= ListenForTerrainWhiteboardActivateGesture;
                StartCoroutine(ReactivateTerrWhiteGesture());
            }
        }
    }

    private void ListenForColorGesture(GestureDetection.EventArgs e)
    {
        if (e.name.Contains("Two"))
        {
            if (e.confidence > Utilities.TwoFingersThreshold && !colorGest && _whiteboardActive)
            {
                // Debug.Log("Color picker triggered");
                colorGest = true;
                gestDetect.OnGesture -= ListenForColorGesture;
                StartCoroutine(ReactivateColorGesture());
            }
        }
    }

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
        else if (!_whiteboardActive && _hideMarkerOnExit && _terrainColorPickerActive)
        {
            ExitTerrainWhiteboard();
        }

        bool whiteboardTrigger = false;
        if (_interaction == InteractionType.Mouse && !TerrainMode)
        {
            whiteboardTrigger = Input.GetKeyDown(KeyCode.T);
        }
        else if (_interaction == InteractionType.Kinect && !TerrainMode)
        {
            whiteboardTrigger = whiteboardGest;
        }

        if (whiteboardTrigger)
        {
            SketchMode = !SketchMode;
        }

        bool terrainWhiteboardTrigger = false;
        if (_interaction == InteractionType.Mouse && !SketchMode && Utilities.ownTerrain)
        {
            terrainWhiteboardTrigger = Input.GetKeyDown(KeyCode.Y);
        }
        else if (_interaction == InteractionType.Kinect && !SketchMode && Utilities.ownTerrain)
        {
            terrainWhiteboardTrigger = terrWhiteGest; // another gesture type here
        }

        if (terrainWhiteboardTrigger)
        {
            TerrainMode = !TerrainMode;
        }

        Transform selObj = cam.transform.gameObject.GetComponent<SelectObject>().GetSelectedObject();

        if (whiteboardTrigger && !selObj)
        {
            TriggerWhiteboard(cam, WhiteboardType.Sketch);
        }
        else if (terrainWhiteboardTrigger && !selObj)
        {
            TriggerWhiteboard(cam, WhiteboardType.Terrain);
        }
    }

    private void TriggerWhiteboard(Camera cam, WhiteboardType type)
    {
        whiteboardGest = false;
        terrWhiteGest = false;
        _whiteboardActive = !_whiteboardActive;

        if (!_whiteboardActive)
        {
            ActivateAllButtons(true);
            cam.transform.position = lastCamPos;
            cam.transform.rotation = lastCamRot;
            if (type == WhiteboardType.Terrain)
            {
                ViewWhiteboard(false, TerrainWhiteboardInstantiated);
            }
            else if (type == WhiteboardType.Sketch)
            {
                ViewWhiteboard(false, WhiteboardInstantiated);
            }

            PenPanel.SetActive(false);
        }
        else if (_whiteboardActive)
        {
            ActivateAllButtons(false);
            lastCamPos = cam.transform.position;
            lastCamRot = cam.transform.rotation;

            GameObject.Find("ObjectsPanel").GetComponent<SpawnObject>().HideObjectsName();

            if (type == WhiteboardType.Sketch)
            {
                _marker.GetComponent<Marker>()._penSize = 15;
                _marker.GetComponent<Marker>()._penImageScale = Utilities.remap(15, 2f, 15f, 12.5f, 25f);
                if (firstTime)
                {
                    WhiteboardInstantiated = Instantiate(WhiteboardPrefab, WhiteboardSpawnPoint.transform);
                    firstTime = false;
                }
                else
                {
                    ViewWhiteboard(true, WhiteboardInstantiated);
                }
            }
            else if (type == WhiteboardType.Terrain)
            {
                if (firstTimeTerrain)
                {
                    TerrainWhiteboardInstantiated = Instantiate(TerrainWhiteboardPrefab, TerrainWhiteboardSpawnPoint.transform);
                    firstTimeTerrain = false;
                }
                else
                {
                    ViewWhiteboard(true, TerrainWhiteboardInstantiated);
                }
            }

            PenPanel.SetActive(true);
            _marker.GetComponent<Marker>().RenderMarker(true);

            if (type == WhiteboardType.Sketch)
            {
                Transform _cameraTransform = WhiteboardInstantiated.transform.Find("CameraPos");
                cam.transform.position = _cameraTransform.position;
                cam.transform.rotation = _cameraTransform.rotation;
            }
            else if (type == WhiteboardType.Terrain)
            {
                Transform _cameraTransform = TerrainWhiteboardInstantiated.transform.Find("CameraPos");
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

        // Trigger apparition of ColorPicker while in whiteboard mode
        bool whitebrdColorTrigger = (_interaction == InteractionType.Mouse) ?
                    Input.GetKeyDown(KeyCode.C) :
                    colorGest;

        if (whitebrdColorTrigger && SketchMode)
        {
            // Debug.Log("Color Picker Triggered");
            colorGest = false;
            _colorPickerActive = !_colorPickerActive;
            _colorPicker.GetComponent<Image>().gameObject.SetActive(_colorPickerActive);

            if (!_colorPickerActive)
            {
                _marker.GetComponent<Marker>().activateMarker(true);

                Utilities.ChangeMarkerColor(_markerTipRenderer.material.color, _markerTipRenderer, _penPanelImage);
            }
        }
        else if (whitebrdColorTrigger && TerrainMode)
        {
            colorGest = false;
            _terrainColorPickerActive = !_terrainColorPickerActive;

            foreach (Transform color in _terrainColorPicker.transform)
            {
                color.gameObject.SetActive(_terrainColorPickerActive);
            }

            if (!_terrainColorPickerActive)
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
    }

    private void ExitTerrainWhiteboard()
    {
        _terrainColorPickerActive = false;
        _hideMarkerOnExit = false;

        foreach (Transform color in _terrainColorPicker.transform)
        {
            color.gameObject.SetActive(_terrainColorPickerActive);
        }
    }


    //     Iterate though each child of the Whiteboard and 
    // disable/enable the Renderer so that when you open it
    // again, the drawings will remain
    public void ViewWhiteboard(bool state, GameObject Whiteboard)
    {
        Whiteboard.GetComponent<Renderer>().enabled = state;
        foreach (Transform component in Whiteboard.transform)
        {
            if (!component.gameObject.name.Equals("CameraPos"))
            {
                component.gameObject.GetComponent<Renderer>().enabled = state;
            }
        }
    }

    private IEnumerator ReactivateWhiteGesture()
    {
        yield return new WaitForSeconds(2);
        gestDetect.OnGesture += ListenForWhiteboardActivateGesture;
    }
    
    private IEnumerator ReactivateTerrWhiteGesture()
    {
        yield return new WaitForSeconds(2);
        gestDetect.OnGesture += ListenForTerrainWhiteboardActivateGesture;
    }
    
    private IEnumerator ReactivateColorGesture()
    {
        yield return new WaitForSeconds(2);
        gestDetect.OnGesture += ListenForColorGesture;
    }
}
