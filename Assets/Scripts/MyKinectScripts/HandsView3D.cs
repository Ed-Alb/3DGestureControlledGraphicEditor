using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

public class HandsView3D : MonoBehaviour
{
    public BodySourceManager mBodySourceManager;
    public GameObject mJointObject;
    public GameObject LHandImg;
    public GameObject RHandImg;
    [SerializeField] private float distanceFromCamera = 6f;

    private GameObject LHand;
    private GameObject RHand;

    private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>();
    private List<JointType> _joints = new List<JointType>
    {
        JointType.HandLeft,
        JointType.HandRight,
    };

    void Update()
    {
        #region Get Kinect Data

        Body[] data = mBodySourceManager.GetData();
        if (data == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null) { continue; }
            if (body.IsTracked) { trackedIds.Add(body.TrackingId); }
        }

        #endregion

        #region Delete Kinect Bodies

        List<ulong> knownIds = new List<ulong>(mBodies.Keys);
        bool delFalg = true;
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                if (delFalg)
                {
                    // Destroy Hands if User got too close to the camera
                    Destroy(LHand);
                    Destroy(RHand);
                    delFalg = false;
                }
                // Destroy Body Object
                Destroy(mBodies[trackingId]);

                // Remove From List
                mBodies.Remove(trackingId);
            }
        }

        #endregion

        #region Create Kinect Bodies

        foreach (var body in data)
        {
            if (body == null) { continue; }
            if (body.IsTracked)
            {
                if (!mBodies.ContainsKey(body.TrackingId))
                {
                    mBodies[body.TrackingId] = CreateBodyObject(body, body.TrackingId);
                }
                UpdateBodyObject(body, mBodies[body.TrackingId]);
            }

            // Tracking for UI Component
            /*if (body.IsTracked)
            {
                KinectInputModule.instance.TrackBody(body);
            }*/
        }

        #endregion

        #region TestHandsPos

        /*if (LHand)
        {
            Debug.Log(LHand.transform.position);
        }

        if (RHand)
        {
            Debug.Log(RHand.transform.position);
        }*/

        #endregion
    }

    private GameObject CreateBodyObject(Body vbody, ulong id)
    {
        // Create body parent
        GameObject body = new GameObject("Body:" + id);
        body.transform.parent = Camera.main.transform;

        body.transform.localRotation = new Quaternion(0, 0, 0, 0);
        body.transform.localPosition = new Vector3(0, 0, 0);
        body.transform.position += body.transform.forward * distanceFromCamera;

        foreach (JointType joint in _joints)
        {
            GameObject newJoint = Instantiate(mJointObject);
            newJoint.GetComponent<Renderer>().enabled = false;
            newJoint.name = joint.ToString();

            newJoint.transform.parent = body.transform;

            //newJoint.AddComponent<KinectUICursor>();
            if (newJoint.name.Equals("HandRight"))
            {
                Vector3 handUIPos = Camera.main.WorldToScreenPoint(newJoint.transform.position);
                RHand = Instantiate(RHandImg, new Vector3(handUIPos.x, handUIPos.y, 0), Quaternion.identity) as GameObject;
                RHand.name = "Right Hand";
                RHand.transform.SetParent(GameObject.Find("UICanvas").transform);
            }
            else if (newJoint.name.Equals("HandLeft"))
            {
                Vector3 handUIPos = Camera.main.WorldToScreenPoint(newJoint.transform.position);
                LHand = Instantiate(LHandImg, new Vector3(handUIPos.x, handUIPos.y, 0), Quaternion.identity) as GameObject;
                LHand.name = "Left Hand";
                LHand.transform.SetParent(GameObject.Find("UICanvas").transform);
            }
        }

        return body;
    }

    private void UpdateBodyObject(Body body, GameObject bodyObject)
    {
        foreach (JointType _joint in _joints)
        {
            Joint sourceJoint = body.Joints[_joint];

            Vector3 targetPosition = GetVector3FromJoint(sourceJoint);
            targetPosition.z = 0;

            Transform jointObject = bodyObject.transform.Find(_joint.ToString());
            //jointObject.localPosition = targetPosition;
            jointObject.localRotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
            jointObject.localPosition = targetPosition;

            Vector3 handUIPos = Camera.main.WorldToScreenPoint(jointObject.transform.position);
            if (jointObject.name.Equals("HandRight"))
            {
                RHand.transform.position = new Vector3(handUIPos.x, handUIPos.y, 0);
            }
            else if (jointObject.name.Equals("HandLeft"))
            {
                LHand.transform.position = new Vector3(handUIPos.x, handUIPos.y, 0);
            }
        }
    }

    private Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

}
