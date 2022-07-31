using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectObject : MonoBehaviour
{
    private string selectedObj = null;
    public static string lastSelectedObj;
    
    private RaycastHit theObj;

    private bool selected = false;
    private bool flashingIn = true;
    private bool changeColor = false;

    private GameObject internalObj;
    private int blueCol;
    public Color32 _objectColor;
    public Color32 oldObjColor;

    public GameObject ScalePanel;
    
    // Will be used on the Sphere and Capsule
    [SerializeField] private Material _sphereOutlineMaterial;
    // Will be used on the Cube and Cylinder
    [SerializeField] private Material _cubeOutlineMaterial;
    [SerializeField] private Material _originalMaterial;

    private void Update()
    {
        if (!WhiteboardHandler._whiteboardActive)
        {
            DetectObject();
        }
    }

    void DetectObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && selectedObj == null)
        {
            if (Physics.Raycast(ray, out theObj))
            {
                if (!Utilities.IsPointerOverUIObject())
                {
                    if (GameObject.Find(theObj.transform.gameObject.name).GetComponent<ObjectInteraction>() != null)
                    {
                        selectedObj = theObj.transform.gameObject.name;
                        lastSelectedObj = theObj.transform.gameObject.name;
                        Camera.main.GetComponent<FreeCam>().TriggerCameraMovement(theObj.transform, true, true, 4f);
                        theObj.transform.gameObject.GetComponent<ObjectInteraction>().SetAction(actionType.Rotate);
                        _objectColor = theObj.transform.gameObject.GetComponent<Renderer>().material.color;

                        if (theObj.transform.gameObject.name.Contains("Sphere") ||
                            theObj.transform.gameObject.name.Contains("Capsule"))
                        {
                            ChangeObjectMaterial(theObj.transform.gameObject, _sphereOutlineMaterial);
                            theObj.transform.gameObject.GetComponent<Renderer>().material.SetColor("_Color", _objectColor);
                        }

                        if (theObj.transform.gameObject.name.Contains("Cube") ||
                            theObj.transform.gameObject.name.Contains("Cylinder"))
                        {
                            ChangeObjectMaterial(theObj.transform.gameObject, _cubeOutlineMaterial);
                            theObj.transform.gameObject.GetComponent<Renderer>().material.SetColor("_Color", _objectColor);
                        }
                    }
                }
            }
        }

        // Exit Selection by pressing C or by doing a gesture (TODO)
        if (Input.GetKeyDown(KeyCode.C) && selectedObj != null)
        {
            if (theObj.transform.gameObject.GetComponent<ObjectInteraction>().GetAction() == actionType.Scale)
            {
                ScalePanel.SetActive(false);
            }
            Camera.main.GetComponent<FreeCam>().setMovementRotation(false, false);
            
            theObj.transform.gameObject.GetComponent<ObjectInteraction>().SetAction(actionType.FreeLook);

            ChangeObjectMaterial(theObj.transform.gameObject, _originalMaterial);
            theObj.transform.gameObject.GetComponent<Renderer>().material.color = _objectColor;

            selectedObj = null;
            GameObject StatePanel = GameObject.Find("StatePanel");
            Animator PanelAnim = StatePanel.GetComponent<Animator>();
            PanelAnim.SetTrigger("TriggerPop");

            theObj = new RaycastHit();
        }
    }

    public void ChangeObjectColor(Color newCol)
    {
        _objectColor = newCol;
    }

    public Color GetObjectColor()
    {
        return _objectColor;
    }

    private void ChangeObjectMaterial(GameObject obj, Material mat)
    {
        obj.transform.gameObject.GetComponent<Renderer>().material = mat;
    }

    public Transform GetSelectedObject()
    {
        if (theObj.transform != null)
        {
            return theObj.transform;
        }

        return null;
    }

    public string GetSelectedObjectName()
    {
        return selectedObj;
    }

    void SwitchColors()
    {
        if (selected)
        {
            internalObj.GetComponent<Renderer>().material.color = new Color32(255, 255, (byte)blueCol, 255);
        }
        else
        {
            internalObj.GetComponent<Renderer>().material.color = oldObjColor;
            changeColor = false;
        }
    }

    public void HandleFlashing()
    {
        GameObject SelectedObj = GameObject.Find(selectedObj);
        if (SelectedObj != null && !selected)
        {
            changeColor = true;
            selected = true;
            internalObj = SelectedObj;
            oldObjColor = SelectedObj.GetComponent<Renderer>().material.color;
            StartCoroutine(Highlight());
        }

        if (SelectedObj == null && selected)
        {
            StopCoroutine(Highlight());
            blueCol = 0;
            selected = false;
        }

        if (internalObj && changeColor)
        {
            SwitchColors();
        }
    }

    IEnumerator Highlight()
    {
        while (selected)
        {
            yield return new WaitForSeconds(.05f);
            if (flashingIn)
            {
                if (blueCol <= 15)
                {
                    flashingIn = false;
                }
                else
                {
                    blueCol -= 10;
                }
            }

            if (!flashingIn)
            {
                if (blueCol >= 250)
                {
                    flashingIn = true;
                }
                else
                {
                    blueCol += 10;
                }
            }
        }
    }
}
