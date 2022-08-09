using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ObjectInteraction : MonoBehaviour
{
    public float maxDist = 20f, minDist = 3f;
    public float rotationSpeed = 10f;
    public float dragSpeed = 1f;

    private float XScale;
    private float YScale;
    private float ZScale;

    private Slider XSlider;
    private Slider YSlider;
    private Slider ZSlider;

    public bool draggingRotation = false;
    public bool draggingTranslation = false;

    private float zCoord;
    private Vector3 mouseOff;

    public actionType action;
    public InteractionType _interaction;

    private Camera cam;
    private string selectedObj;

    #region FloatingText Variables

    public GameObject FloatingTextPrefab;
    public bool hideText = false;
    public GameObject FloatingText;

    public event EventHandler OnTextKeyPress;

    #endregion

    private GestureDetection gestDetect;
    private bool ObjectNameTwoFingersAction = false;

    private HandsView3D handsView3D;
    private bool moveTowards = false;
    private bool moveAway = false;

    private bool scaleZPos = false;
    private bool scaleZNeg = false;

    private float HorizontalRotationSignal;
    private float VerticalRotationSignal;

    private GameObject ArrowsHolder;

    private KinectHandsEvents handsEvents;

    private void Start()
    {
        _interaction = Utilities._interaction;

        action = actionType.FreeLook;
        cam = Camera.main;

        XScale = transform.localScale[0];
        YScale = transform.localScale[1];
        ZScale = transform.localScale[2];

        if (_interaction == InteractionType.Kinect)
        {
            gestDetect = GameObject.Find("GestureDetectHandler").GetComponent<GestureDetection>();
            if (gestDetect)
            {
                gestDetect.OnGesture += ListenForObjectNameActivateGesture;
            }

            handsView3D = GameObject.Find("HandsManager").GetComponent<HandsView3D>();
            if (handsView3D)
            {
                handsView3D.depthDecode += ListenForDragDepth;
                handsView3D.depthDecode += ListenForScaleDepth;
            }

            handsEvents = GameObject.Find("GestureDetectHandler").GetComponent<KinectHandsEvents>();

            ArrowsHolder = GameObject.Find("ArrowsHolder");
        }

        OnTextKeyPress += TriggerObjectsName;
    }

    #region Listeners For Depth Changes and Objects Name Activation
    private void ListenForDragDepth(HandsView3D.DepthEventArgs e)
    {
        bool selected = selectedObj != null && selectedObj.Equals(this.name);
        if (selected && action == actionType.Drag)
        {
            if (handsEvents.GetLAction())
            {
                if (e.handName.Equals("Left"))
                {
                    if (e.actionType.Contains("Normal"))
                    {
                        //Debug.Log("Left Hand Normal");
                        moveAway = false;
                        moveTowards = false;
                    }
                    else if (e.actionType.Contains("Forward"))
                    {
                        moveAway = true;
                    }
                    else if (e.actionType.Contains("Backward"))
                    {
                        moveTowards = true;
                    }
                }
            }
            else
            {
                moveAway = false;
                moveTowards = false;
            }
        }
    }

    private void ListenForScaleDepth(HandsView3D.DepthEventArgs e)
    {
        bool selected = selectedObj != null && selectedObj.Equals(this.name);
        if (selected && action == actionType.Scale)
        {
            if (handsEvents.GetRAction())
            {
                if (e.handName.Equals("Left"))
                {
                    if (e.actionType.Contains("Normal"))
                    {
                        scaleZPos = false;
                        scaleZNeg = false;
                    }
                    else if (e.actionType.Contains("Forward"))
                    {
                        scaleZPos = true;
                    }
                    else if (e.actionType.Contains("Backward"))
                    {
                        scaleZNeg = true;
                    }
                }
            }
            else
            {
                scaleZPos = false;
                scaleZNeg = false;
            }
        }
    }

    private void ListenForObjectNameActivateGesture(GestureDetection.EventArgs e)
    {
        if (e.name.Contains("Two"))
        {
            if (e.confidence > Utilities.TwoFingersThreshold &&
                !cam.GetComponent<SelectObject>().GetSelectedObject() &&
                !ObjectNameTwoFingersAction && !WhiteboardHandler._whiteboardActive)
            {
                // Debug.Log("Color picker triggered");
                ObjectNameTwoFingersAction = true;
                gestDetect.OnGesture -= ListenForObjectNameActivateGesture;
                StartCoroutine(ReactivateObjectNameGesture());
            }
        }
    }

    #endregion

    private void Update()
    {
        selectedObj = cam.GetComponent<SelectObject>().GetSelectedObjectName();

        if (_interaction == InteractionType.Mouse && Input.GetMouseButtonUp(0))
        {
            this.draggingRotation = false;
            this.draggingTranslation = false;
        }

        if (_interaction == InteractionType.Kinect && selectedObj != null && name.Equals(selectedObj))
        {
            this.draggingRotation = true;
            this.draggingTranslation = true;
        }

        if (action == actionType.Rotate && selectedObj.Equals(name))
        {
            if (_interaction == InteractionType.Mouse && this.draggingRotation &&
                !Utilities.IsPointerOverUIObject())
            {
                HandleMouseRotation();
            }
            else if (_interaction == InteractionType.Kinect)
            {
                HandleHandsRotation();
            }
        }

        if (action == actionType.Drag && selectedObj.Equals(name))
        {
            moveObjectTowardsCamera();

            if (_interaction == InteractionType.Mouse && this.draggingTranslation
                && !Utilities.IsPointerOverUIObject())
            {
                HandleDragging(_interaction, Input.mousePosition);
            }
            else if (_interaction == InteractionType.Kinect &&
                !Utilities.IsHandOverUIObject(GameObject.Find("Right Hand").transform))
            {
                HandleDragging(_interaction, handsView3D.RightHandPosition());
            }
        }

        if (action == actionType.Scale && selectedObj.Equals(name))
        {
            XSlider = GameObject.Find("X-Axis").GetComponent<Slider>();
            YSlider = GameObject.Find("Y-Axis").GetComponent<Slider>();
            ZSlider = GameObject.Find("Z-Axis").GetComponent<Slider>();

            HandleScaling(_interaction, Time.deltaTime);
        }

        HandleObjectsName();
    }

    #region Object and Camera Rotation (For Kinect Only) Logic
    private void HandleMouseRotation()
    {
        float x = Input.GetAxis("Mouse X") * rotationSpeed;
        float y = Input.GetAxis("Mouse Y") * rotationSpeed;

        Vector3 right = Vector3.Cross(cam.transform.up, transform.position - cam.transform.position);
        Vector3 up = Vector3.Cross(transform.position - cam.transform.position, right);

        transform.rotation = Quaternion.AngleAxis(-x, up) * transform.rotation;
        transform.rotation = Quaternion.AngleAxis(y, right) * transform.rotation;
    }

    private void HandleHandsRotation()
    {
        if (handsEvents.GetRAction() ^ handsEvents.GetLAction())
        {
            // Daca e mana stanga inchisa, atunci apar in stanga 4 butoane cu sageti
            // pe care daca se sta cu mana dreapta se va da un semnal obiectului
            // ca trebuie sa se roteasca
            if (handsEvents.GetRAction() && 
                !Utilities.IsHandOverUIObject(GameObject.Find("Right Hand").transform))
            {
                SetArrowsPos(-1);
                SetArrowsActive(true);

                // Debug.Log(HorizontalRotationSignal + " " + VerticalRotationSignal);
                float x = VerticalRotationSignal * rotationSpeed;
                float y = HorizontalRotationSignal * rotationSpeed;

                Vector3 right = Vector3.Cross(cam.transform.up, transform.position - cam.transform.position);
                Vector3 up = Vector3.Cross(transform.position - cam.transform.position, right);

                transform.rotation = Quaternion.AngleAxis(-x, up) * transform.rotation;
                transform.rotation = Quaternion.AngleAxis(y, right) * transform.rotation;
            }

            if (handsEvents.GetLAction() &&
                !Utilities.IsHandOverUIObject(GameObject.Find("Left Hand").transform))
            {
                SetArrowsPos(1);
                SetArrowsActive(true);

                cam.GetComponent<FreeCam>().RotateAroundObject(
                    transform, Vector3.Distance(transform.position, cam.transform.position),
                    HorizontalRotationSignal, -VerticalRotationSignal
                );
            }
        }
        else
        {
            SetArrowsActive(false);
        }
    }

    private void SetArrowsPos(int dir)
    {
        RectTransform ArrowsRT = ArrowsHolder.GetComponent<RectTransform>();
        ArrowsRT.localPosition = dir * Vector3.right * 500;
    }

    public void SetArrowsActive(bool state)
    {
        foreach (Transform arrow in ArrowsHolder.transform)
        {
            arrow.gameObject.SetActive(state);
        }
    }

    #endregion

    #region Object Movement / Dragging Logic + Activation of rotation On Mouse Drag
    private void HandleDragging(InteractionType interaction, Vector3 CursorPos)
    {
        //transform.position = getMouseWorldPos() + mouseOff;
        if (interaction == InteractionType.Mouse)
        {
            // Debug.Log("Mouse");
            Vector3 MousePos = CursorPos;
            Vector3 ScreenPos = new Vector3(MousePos.x, MousePos.y, cam.WorldToScreenPoint(transform.position).z);
            Vector3 NewWorldPos = cam.ScreenToWorldPoint(ScreenPos);
            transform.position = NewWorldPos;
        }
        else if (interaction == InteractionType.Kinect)
        {
            if (handsEvents.GetRAction())
            {
                // Debug.Log("Right Hand Move");
                Vector3 RHandPos = CursorPos;
                Vector3 ScreenPos = new Vector3(RHandPos.x, RHandPos.y, cam.WorldToScreenPoint(transform.position).z);
                Vector3 NewWorldPos = cam.ScreenToWorldPoint(ScreenPos);
                transform.position = NewWorldPos;
            }
        }
    }

    private void OnMouseDrag()
    {
        if (selectedObj != null && selectedObj.Equals(this.name) && 
            _interaction == InteractionType.Mouse)
        {
            switch (action)
            {
                case actionType.Drag:
                    draggingTranslation = true;
                    break;

                case actionType.Rotate:
                    draggingRotation = true;
                    break;
            }
        }
    }
    #endregion

    #region Object Scaling logic
    private void HandleScaling(InteractionType interaction, float deltaTime)
    {
        if (interaction == InteractionType.Mouse)
        {
            XScale = XSlider.value;
            YScale = YSlider.value;
            ZScale = ZSlider.value;
            transform.localScale = new Vector3(XScale, YScale, ZScale);
        }
        else if (interaction == InteractionType.Kinect)
        {
            float scaleSpeed = 1.5f;
            if (handsEvents.GetRAction() && handsEvents.GetLAction() &&
                (!Utilities.IsHandOverUIObject(GameObject.Find("Right Hand").transform) ||
                !Utilities.IsHandOverUIObject(GameObject.Find("Left Hand").transform)))
            {
                if (handsView3D.RightHandPosition().y < Screen.height / 2.5 &&
                    handsView3D.LeftHandPosition().y < Screen.height / 2.5)
                {
                    ZoomObject(-scaleSpeed, deltaTime);
                }
                else if (handsView3D.RightHandPosition().y > 2.5 * Screen.height / 4 &&
                    handsView3D.LeftHandPosition().y > 2.5 * Screen.height / 4)
                {
                    ZoomObject(scaleSpeed, deltaTime);
                }
            }
            else if (handsEvents.GetRAction() &&
                !Utilities.IsHandOverUIObject(GameObject.Find("Right Hand").transform))
            {
                // Daca mana dreapta e stransa, atunci asculta
                // mana stanga in adancime pentru a scala pe Z
                if (scaleZNeg)
                {
                    ZScale = ChangeAxisScale(ZSlider, ZScale, -scaleSpeed, deltaTime);
                }
                else if (scaleZPos)
                {
                    ZScale = ChangeAxisScale(ZSlider, ZScale, scaleSpeed, deltaTime);
                }
            }
            else if (handsEvents.GetLAction() &&
                !Utilities.IsHandOverUIObject(GameObject.Find("Left Hand").transform))
            {
                // Daca mana stanga e stransa, atunci asculta mana
                // dreapta pe X si Y cumva pentru a scala pe aceste axe
                if (handsView3D.RightHandPosition().y < Screen.height / 3)
                {
                    YScale = ChangeAxisScale(YSlider, YScale, -scaleSpeed, deltaTime);
                }
                else if (handsView3D.RightHandPosition().y > 3 * Screen.height / 4)
                {
                    YScale = ChangeAxisScale(YSlider, YScale, scaleSpeed, deltaTime);
                }

                if (handsView3D.LeftHandPosition().x < Screen.width / 4)
                {
                    XScale = ChangeAxisScale(XSlider, XScale, -scaleSpeed, deltaTime);
                }
                else if (handsView3D.LeftHandPosition().x > .75 * Screen.width / 2)
                {
                    XScale = ChangeAxisScale(XSlider, XScale, scaleSpeed, deltaTime);
                }
            }
            transform.localScale = new Vector3(XScale, YScale, ZScale);
        }
    }

    private void ZoomObject(float OverallScaleSpeed, float deltaTime)
    {
        XScale += OverallScaleSpeed * deltaTime;
        YScale += OverallScaleSpeed * deltaTime;
        ZScale += OverallScaleSpeed * deltaTime;

        XScale = Mathf.Clamp(XScale, XSlider.minValue, XSlider.maxValue);
        YScale = Mathf.Clamp(YScale, YSlider.minValue, YSlider.maxValue);
        ZScale = Mathf.Clamp(ZScale, ZSlider.minValue, ZSlider.maxValue);

        XSlider.value = XScale;
        YSlider.value = YScale;
        ZSlider.value = ZScale;
    }

    private float ChangeAxisScale(Slider AxisSLider, float AxisScale, float AxisScaleSpeed, float deltaTime)
    {
        AxisScale += AxisScaleSpeed * deltaTime;
        AxisScale = Mathf.Clamp(AxisScale, AxisSLider.minValue, AxisSLider.maxValue);
        AxisSLider.value = AxisScale;
        return AxisScale;
    }

    #endregion

    private void moveObjectTowardsCamera()
    {
        if (selectedObj != null)
        {
            if (selectedObj.Equals(this.name))
            {
                GameObject selObj = GameObject.Find(selectedObj);
                Vector3 objPlayerDir = selObj.transform.position - Camera.main.transform.position;
                float objPlrDistance = Vector3.Magnitude(objPlayerDir);

                bool awayAction = false, towardsAction = false;
                awayAction = _interaction == InteractionType.Kinect ? moveAway : Input.GetKey(KeyCode.Z);
                towardsAction = _interaction == InteractionType.Kinect ? moveTowards : Input.GetKey(KeyCode.X);

                if (awayAction)
                {
                    if (objPlrDistance < maxDist)
                    {
                        selObj.transform.position += objPlayerDir * dragSpeed * Time.deltaTime;
                    }
                }

                if (towardsAction)
                {
                    if (objPlrDistance > minDist)
                    {
                        selObj.transform.position -= objPlayerDir * dragSpeed * Time.deltaTime;
                    }
                }
            }
        }
    }

    #region Object Name Zone

    public bool getHideText() {
        return hideText;
    }
    
    public bool setHideText(bool hideText) {
        this.hideText = hideText;
        return hideText;
    }

    public GameObject getFloatingText()
    {
        return FloatingText;
    }

    private void HandleObjectsName()
    {
        if (Input.GetKeyDown(KeyCode.P) || ObjectNameTwoFingersAction)
        {
            ObjectNameTwoFingersAction = false;
            OnTextKeyPress?.Invoke(this, EventArgs.Empty);
        }
    }

    private void TriggerObjectsName(object sender, EventArgs e)
    {
        hideText = !hideText;
        if (hideText && FloatingTextPrefab)
        {
            FloatingText = ShowObjectName();
        }
        else if (!hideText)
        {
            Destroy(FloatingText);
        }
    }

    private GameObject ShowObjectName()
    {
        GameObject goName = Instantiate(FloatingTextPrefab, transform.position, Quaternion.identity, transform);
        goName.GetComponent<TextMesh>().text = name;

        if (name.Contains("Clone"))
        {
            goName.GetComponent<TextMesh>().text = Utilities.RenameClonedObject(goName.GetComponent<TextMesh>().text);
        }
        return goName;
    }

    #endregion

    #region Setters and Getters
    public void SetAction(actionType action)
    {
        this.action = action;
    }

    public actionType GetAction()
    {
        return this.action;
    }

    public Vector3 GetScale()
    {
        return new Vector3(XScale, YScale, ZScale);
    }

    public void TriggerHorizontalSignal(float val)
    {
        HorizontalRotationSignal = val;
    }
    
    public void TriggerVerticalSignal(float val)
    {
        VerticalRotationSignal = val;
    }

    #endregion

    private IEnumerator ReactivateObjectNameGesture()
    {
        yield return new WaitForSeconds(2);
        gestDetect.OnGesture += ListenForObjectNameActivateGesture;
    }
}
