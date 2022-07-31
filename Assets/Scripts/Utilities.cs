using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum actionType
{
    FreeLook,
    Drag,
    Rotate,
    Scale,
    Deform
}

public class Utilities : MonoBehaviour
{
    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public static float remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
    {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }

    public static string RenameClonedObject(string _oldObjName)
    {
        string _newButtonName = _oldObjName.Remove(0, "Interactive".Length);
        int _cloneWordPos = _newButtonName.IndexOf("(Clone)");
        _newButtonName = _newButtonName.Remove(_cloneWordPos, "(Clone)".Length);

        return _newButtonName;
    }

    public static void ChangeMarkerColor(Color c, Renderer _markerTipRenderer, GameObject _penPanelImage)
    {
        PreviewMarkerPanelColor(c, _penPanelImage);
        _markerTipRenderer.material.color = c;
    }

    public static void ChangeObjectColor(Color c)
    {
        Transform objTransform = Camera.main.GetComponent<SelectObject>().GetSelectedObject();
        if (objTransform)
        {
            PreviewObjectMaterialColor(objTransform, c);
            Camera.main.gameObject.GetComponent<SelectObject>().ChangeObjectColor(c);
        }
    }

    public static void PreviewObjectMaterialColor(Transform objTransform, Color c)
    {
        var r = objTransform.gameObject.GetComponent<MeshRenderer>();
        r.material.color = c;
    }

    public static void PreviewMarkerPanelColor(Color c, GameObject _penPanelImage)
    {
        _penPanelImage.GetComponent<Image>().color = c;
    }
}
