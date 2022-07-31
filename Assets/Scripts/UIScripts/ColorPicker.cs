using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[Serializable]
public class ColorEvent : UnityEvent<Color> { }

public class ColorPicker : MonoBehaviour
{
    private Camera cam;
    private bool _active = false;

    RectTransform _rect;
    Texture2D _colorTexture;

    [SerializeField]
    private GameObject _markerTip;
    private Renderer _markerTipRenderer;

    [SerializeField]
    private GameObject _marker;

    [SerializeField]
    private GameObject _penPanelImage;

    public ColorEvent OnColorSelect;

    void Start()
    {
        cam = Camera.main;

        _rect = GetComponent<RectTransform>();

        _colorTexture = GetComponent<Image>().mainTexture as Texture2D;

        _markerTipRenderer = _markerTip.GetComponent<Renderer>();
    }


    void Update()
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(_rect, Input.mousePosition))
        {
            if (WhiteboardHandler._whiteboardActive)
            {
                _marker.GetComponent<Marker>().activateMarker(false);
            }

            HandleColorSelection();
        }
        else
        {
            if (WhiteboardHandler._whiteboardActive)
            {
                _marker.GetComponent<Marker>().activateMarker(true);
                _penPanelImage.GetComponent<Image>().color = _markerTipRenderer.material.color;
            }
            else
            {
                Transform objTransform = cam.GetComponent<SelectObject>().GetSelectedObject();
                if (objTransform)
                {
                    var r = objTransform.gameObject.GetComponent<MeshRenderer>();
                    r.material.color = cam.GetComponent<SelectObject>()._objectColor;
                }
            }
        }
    }

    public void HandleColorSelection()
    {
        Vector2 delta;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, Input.mousePosition, null, out delta);
        float width = _rect.rect.width;
        float height = _rect.rect.height;
        delta += new Vector2(width * .5f, height * .5f);

        float x = Mathf.Clamp(delta.x / width, 0f, 1f);
        float y = Mathf.Clamp(delta.y / height, 0f, 1f);
        // Debug.Log(x, y);
        int texX = Mathf.RoundToInt(x * _colorTexture.width);
        int texY = Mathf.RoundToInt(y * _colorTexture.height);

        Color c = _colorTexture.GetPixel(texX, texY);

        // Preview Color
        if (!WhiteboardHandler._whiteboardActive)
        {
            // What to do if the colorPicker is used on an object
            Transform objTransform = cam.GetComponent<SelectObject>().GetSelectedObject();
            if (objTransform)
            {
                 Utilities.PreviewObjectMaterialColor(objTransform, c);
            }
        }
        else
        {
            Utilities.PreviewMarkerPanelColor(c, _penPanelImage);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (WhiteboardHandler._whiteboardActive)
            {
                // What to do if the colorPicker is used on the marker
                Utilities.ChangeMarkerColor(c, _markerTipRenderer, _penPanelImage);
            }
            else
            {
                // What to do if the colorPicker is used on an object
                Utilities.ChangeObjectColor(c);
            }
        }
    }
}
