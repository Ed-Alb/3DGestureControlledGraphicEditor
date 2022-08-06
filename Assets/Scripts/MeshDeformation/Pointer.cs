using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Pointer : MonoBehaviour
{
    public float distance = 100;
    public float radius = 0.2f;
    public float force = 0.3f;

    private Camera cam;

    private InteractionType interaction;
    private KinectHandsEvents handsEvents;

    private void Start()
    {
        interaction = Utilities._interaction;

        if (interaction == InteractionType.Kinect)
        {
            handsEvents = GameObject.Find("GestureDetectHandler").GetComponent<KinectHandsEvents>();
        }
    }

    private void Awake()
    {
        cam = gameObject.GetComponent<Camera>();
    }

    private void Update()
    {
        // Create a ray from the screen point where the mouse is to the scene.
        Ray ray = new Ray();
        if (interaction == InteractionType.Mouse)
        {
            ray = cam.ScreenPointToRay(Input.mousePosition);
        }
        else if (interaction == InteractionType.Kinect)
        {
            GameObject RHand = GameObject.Find("Right Hand");
            GameObject LHand = GameObject.Find("Left Hand");

            if (RHand && handsEvents.GetRAction())
            {
                ray = cam.ScreenPointToRay(RHand.transform.position);
            }
            else if (LHand && handsEvents.GetLAction())
            {
                ray = cam.ScreenPointToRay(LHand.transform.position);
            }
            else if (RHand || LHand)
            {
                ray = cam.ScreenPointToRay(RHand.transform.position);
            }
        }

        bool rayTrigger = (interaction == InteractionType.Mouse) ?
            Input.GetMouseButton(0) || Input.GetMouseButton(1) :
            handsEvents.GetRAction() || handsEvents.GetLAction();

        // Check if the left mouse button is held down.
        if (!rayTrigger)
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance);
            return;
        }

        // Check if the ray hit a gameobject with the mesh deformer component.
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance))
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            ObjectInteraction hitObject = hit.collider.GetComponent<ObjectInteraction>();
            actionType currAction = actionType.FreeLook;
            if (hitObject)
            {
                currAction = hitObject.GetAction();
            }
            if (deformer && currAction == actionType.Deform)
            {
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, Color.green);
                if (interaction == InteractionType.Mouse)
                {
                    if (Input.GetMouseButton(0))
                    {
                        deformer.Deform(hit.point, radius, force);
                    }
                    else if (Input.GetMouseButton(1))
                    {
                        deformer.Deform(hit.point, radius, -force);
                    }
                }
                else if (interaction == InteractionType.Kinect)
                {
                    if (handsEvents.GetRAction())
                    {
                        deformer.Deform(hit.point, radius, force);
                    }
                    else if (handsEvents.GetLAction())
                    {
                        deformer.Deform(hit.point, radius, -force);
                    }
                }
            }
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance);
        }
    }
}