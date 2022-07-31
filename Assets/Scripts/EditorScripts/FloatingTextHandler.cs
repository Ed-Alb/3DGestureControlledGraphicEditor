using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextHandler : MonoBehaviour
{
    private Vector3 Offset = new Vector3(0, .25f, 0);

    void Start()
    {
        transform.localPosition += Offset;
        LookTowardsCamera();
    }

    void Update()
    {
        LookTowardsCamera();
    }

    private void LookTowardsCamera()
    {
        // Look  toards the camera
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
        // Rotate so that the text is not inverted
        transform.Rotate(0, 180, 0);
    }
}
