using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

public class KinectTest : MonoBehaviour
{
    public BodySourceManager mBodySourceManager;
    public GameObject mJointObject;
    [SerializeField] private float distanceFromCamera = 6f;

    private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>();
    private List<JointType> _joints = new List<JointType>
    {
        JointType.HandLeft,
        JointType.HandRight,
        //JointType.Head,
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
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
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
        }

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
            newJoint.name = joint.ToString();

            newJoint.transform.parent = body.transform;
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
        }
    }

    private Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

}
