using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectObject : MonoBehaviour
{
    #region Old Selection variables <- Useless now!
    private int blueCol;
    private Color32 oldObjColor;
    private bool selected = false;
    #endregion

    private string selectedObj = null;
    public static string lastSelectedObj;
    
    private RaycastHit theObj;

    private bool flashingIn = true;
    private bool changeColor = false;

    private GameObject internalObj;

    public Color32 _objectColor;

    public GameObject ScalePanel;
    
    // Will be used on the Sphere and Capsule
    [SerializeField] private Material _sphereOutlineMaterial;
    // Will be used on the Cube and Cylinder
    [SerializeField] private Material _cubeOutlineMaterial;
    [SerializeField] private Material _originalMaterial;

    private InteractionType _interaction;

    private void Start()
    {
        _interaction = Utilities._interaction;
    }

    private void Update()
    {
        if (!WhiteboardHandler._whiteboardActive)
        {
            DetectObject();
        }
    }

    void DetectObject()
    {
        // Make it so that the user can choose at the beginning if he wants to
        // use the kinect sensor or the mouse -- TODO
        if (_interaction == InteractionType.Mouse)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            HandleMouseDetection(ray);
        }
        else if (_interaction == InteractionType.Kinect &&
            (GameObject.Find("Left Hand") || GameObject.Find("Right Hand")))
        {
            GameObject LeftHand = GameObject.Find("Left Hand");
            GameObject RightHand = GameObject.Find("Right Hand");
            HandleHandsDetection(LeftHand, RightHand);
        }
    }

    public void HandleMouseDetection(Ray ray)
    {
        // Select Object by Clicking it
        if (Input.GetMouseButtonDown(0) && selectedObj == null)
        {
            ObjectSelection(ray);
        }

        // Exit Selection by pressing C
        if (Input.GetKeyDown(KeyCode.C) && selectedObj != null)
        {
            ObjectDeselection();
        }
    }

    public void HandleHandsDetection(GameObject LeftHand, GameObject RightHand)
    {
        Ray leftHandRay = Camera.main.ScreenPointToRay(LeftHand.GetComponent<KinectUICursor>().GetHandCursorPosition());
        Ray rightHandRay = Camera.main.ScreenPointToRay(RightHand.GetComponent<KinectUICursor>().GetHandCursorPosition());

        bool rightHandAction = RightHand.GetComponent<KinectUICursor>().IsPerformingAction();
        bool leftHandAction = LeftHand.GetComponent<KinectUICursor>().IsPerformingAction();

        // Select Object if hand action was performed only with one hand (the one that was on the object)
        // because if I let it consider both hands, 2 objects will be detected and one will be null or something
        // else (depending on where the other hand was)
        if ((rightHandAction ^ leftHandAction) && selectedObj == null)
        {
            if (rightHandAction && !Utilities.IsHandOverUIObject(RightHand.transform))
            {
                ObjectSelection(rightHandRay);
            }

            if (leftHandAction && !Utilities.IsHandOverUIObject(LeftHand.transform))
            {
                ObjectSelection(leftHandRay);
            }
        }

        bool HandsCloseAction = GameObject.Find("GestureDetectHandler").GetComponent<KinectHandsEvents>().GetCloseAction();
        if (HandsCloseAction && selectedObj != null)
        {
            ObjectDeselection();
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

    private void ObjectSelection(Ray ray)
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

    private void ObjectDeselection()
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

        if (_interaction == InteractionType.Kinect)
        {
            theObj.transform.gameObject.GetComponent<ObjectInteraction>().SetArrowsActive(false);
        }

        theObj = new RaycastHit();
    }


    #region Old Selection Method <- Useless Now!
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
    #endregion
}
