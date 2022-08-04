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
    private InteractionType interaction;

    private Camera cam;
    private string selectedObj;

    public GameObject FloatingTextPrefab;
    public bool hideText = false;
    public GameObject FloatingText;

    public event EventHandler OnTextKeyPress;

    private GestureDetection gestDetect;
    private bool ObjectNameTwoFingrsAction = false;

    private HandsView3D handsViewDepth;
    private bool moveTowards = false;
    private bool moveAway = false;

    private KinectHandsEvents handsEvents;

    private void Start()
    {
        interaction = InteractionType.Mouse;
        action = actionType.FreeLook;
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        XScale = transform.localScale[0];
        YScale = transform.localScale[1];
        ZScale = transform.localScale[2];

        gestDetect = GameObject.Find("GestureDetectHandler").GetComponent<GestureDetection>();
        if (gestDetect)
        {
            gestDetect.OnGesture += ListenForObjectNameActivateGesture;
        }

        handsViewDepth = GameObject.Find("HandsManager").GetComponent<HandsView3D>();
        if (handsViewDepth)
        {
            handsViewDepth.depthDecode += ListenForDepth;
        }

        handsEvents = GameObject.Find("GestureDetectHandler").GetComponent<KinectHandsEvents>();

        OnTextKeyPress += TriggerObjectsName;
    }

    private void ListenForDepth(HandsView3D.DepthEventArgs e)
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

    private void ListenForObjectNameActivateGesture(GestureDetection.EventArgs e)
    {
        if (e.name.Contains("Two"))
        {
            if (e.confidence > .1f && !ObjectNameTwoFingrsAction && !WhiteboardHandler._whiteboardActive &&
                !cam.GetComponent<SelectObject>().GetSelectedObject())
            {
                // Debug.Log("Color picker triggered");
                ObjectNameTwoFingrsAction = true;
                gestDetect.OnGesture -= ListenForObjectNameActivateGesture;
                StartCoroutine(ReactivateObjectNameGesture());
            }
        }
    }

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

    private void Update()
    {
        selectedObj = cam.GetComponent<SelectObject>().GetSelectedObjectName();

        if (Input.GetMouseButtonUp(0))
        {
            this.draggingRotation = false;
            this.draggingTranslation = false;
        }

        if (interaction == InteractionType.Kinect && selectedObj != null && name.Equals(selectedObj))
        {
            this.draggingRotation = true;
            this.draggingTranslation = true;
        }

        if (!Utilities.IsPointerOverUIObject())
        {
            if (this.draggingRotation)
            {
                if (action == actionType.Rotate)
                {
                    HandleRotation();
                }
            }

            if (this.draggingTranslation)
            {
                if (action == actionType.Drag)
                {
                    if (interaction == InteractionType.Mouse)
                    {
                        HandleMouseDragging();
                    }
                    else if (interaction == InteractionType.Kinect)
                    {
                        Debug.Log("Kinect Drag");
                        HandleKinectDragging();
                    }
                }
            }
        }

        if (action == actionType.Scale)
        {
            XSlider = GameObject.Find("X-Axis").GetComponent<Slider>();
            YSlider = GameObject.Find("Y-Axis").GetComponent<Slider>();
            ZSlider = GameObject.Find("Z-Axis").GetComponent<Slider>();

            HandleScaling();
        }

        if (this.draggingTranslation == false)
        {
            moveObjectTowardsCamera();
        }

        HandleObjectsName();
    }

    private void HandleObjectsName()
    {
        if (Input.GetKeyDown(KeyCode.P) || ObjectNameTwoFingrsAction)
        {
            ObjectNameTwoFingrsAction = false;
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

    private void HandleScaling()
    {
        if (selectedObj == name)
        {
            XScale = XSlider.value;
            YScale = YSlider.value;
            ZScale = ZSlider.value;
            transform.localScale = new Vector3(XScale, YScale, ZScale);
        }
    }

    private void HandleRotation()
    {
        float x = Input.GetAxis("Mouse X") * rotationSpeed;
        float y = Input.GetAxis("Mouse Y") * rotationSpeed;

        Vector3 right = Vector3.Cross(cam.transform.up, transform.position - cam.transform.position);
        Vector3 up = Vector3.Cross(transform.position - cam.transform.position, right);

        transform.rotation = Quaternion.AngleAxis(-x, up) * transform.rotation;
        transform.rotation = Quaternion.AngleAxis(y, right) * transform.rotation;
    }

    private void HandleMouseDragging()
    {
        transform.position = getMouseWorldPos() + mouseOff;
    }

    private void HandleKinectDragging()
    {
        /*if (handsEvents.GetRAction())
        {
            Debug.Log("Should Move");
            Vector3 RightHandPos = cam.ScreenToWorldPoint(handsViewDepth.RightHandPosition());
            transform.position = new Vector3(RightHandPos.x, RightHandPos.y, transform.position.z);
        }*/
    }

    private void OnMouseDown()
    {
        zCoord = cam.WorldToScreenPoint(gameObject.transform.position).z;
        mouseOff = gameObject.transform.position - getMouseWorldPos();
    }

    private Vector3 getMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;

        mousePoint.z = zCoord;

        return cam.ScreenToWorldPoint(mousePoint);
    }

    private void OnMouseDrag()
    {
        if (selectedObj != null)
        {
            if (selectedObj.Equals(this.name))
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
    }


    private void moveObjectTowardsCamera()
    {
        if (selectedObj != null)
        {
            if (selectedObj.Equals(this.name))
            {
                GameObject selObj = GameObject.Find(selectedObj);
                Vector3 objPlayerDir = selObj.transform.position - Camera.main.transform.position;
                float objPlrDistance = Vector3.Magnitude(objPlayerDir);

                if (Input.GetKey(KeyCode.Z) || moveAway)
                {
                    if (objPlrDistance < maxDist)
                    {
                        selObj.transform.position += objPlayerDir * dragSpeed * Time.deltaTime;
                    }
                }

                if (Input.GetKey(KeyCode.X) || moveTowards)
                {
                    if (objPlrDistance > minDist)
                    {
                        selObj.transform.position -= objPlayerDir * dragSpeed * Time.deltaTime;
                    }
                }
            }
        }
    }

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

    private IEnumerator ReactivateObjectNameGesture()
    {
        yield return new WaitForSeconds(2);
        gestDetect.OnGesture += ListenForObjectNameActivateGesture;
    }
}
