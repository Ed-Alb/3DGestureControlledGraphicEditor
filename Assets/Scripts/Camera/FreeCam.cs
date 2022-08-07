using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class FreeCam : MonoBehaviour
{
    public float movementSpeed = 10f;

    public float fastMovementSpeed = 20f;

    public float freeLookSensitivity = 3f;

    public float zoomSensitivity = 10f;

    public float fastZoomSensitivity = 50f;

    private bool looking = false;

    private bool mustMove = false;
    private bool mustRotate = false;

    private Vector3 target;
    private Quaternion oldRotation;
    private float moveTowardsSpeed = 3f;
    private float rotTowardsSpeed = 3f;

    private float pitch = 0.0f;
    private float yaw = 0.0f;

    // Default distance at which camera stops near object
    private float dist = 4f;

    private float _rotX, _rotY;
    private Vector3 _currentRot;
    private Vector3 _smoothVelocity = Vector3.zero;
    private float _smoothTime = .2f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        HandleFocusOnObject();

        if (mustMove || mustRotate || WhiteboardHandler._whiteboardActive)
        {
            return;
        }

        HandleCameraMovement();

        if (looking)
        {
            yaw += Input.GetAxis("Mouse X") * freeLookSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * freeLookSensitivity;
            transform.localEulerAngles = new Vector3(pitch, yaw, 0f);
        }

        /*float axis = Input.GetAxis("Mouse ScrollWheel");
        if (axis != 0 && !IsPointerOverUIObject())
        {
            var zoomSensitivity = fastMode ? this.fastZoomSensitivity : this.zoomSensitivity;
            transform.position = transform.position + transform.forward * axis * zoomSensitivity;
        }*/

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartLooking();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            StopLooking();
        }
    }

    // Brings Camera closer to the desired object
    private void HandleFocusOnObject()
    {
        if (mustMove)
        {
            MoveTowardsObject(target);
        }

        if (mustRotate)
        {
            RotateTowardsObject(target);
        }
    }

    // Movement in the scene
    private void HandleCameraMovement()
    {
        var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        var _movementSpeed = fastMode ? this.fastMovementSpeed : this.movementSpeed;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position = transform.position + (-transform.right * _movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.position = transform.position + (transform.right * _movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            transform.position = transform.position + (transform.forward * _movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            transform.position = transform.position + (-transform.forward * _movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.position = transform.position + (transform.up * _movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.position = transform.position + (-transform.up * _movementSpeed * Time.deltaTime);
        }
    }

    void OnDisable()
    {
        StopLooking();
    }

    public void StartLooking()
    {
        looking = true;
        Cursor.visible = false;
    }

    public void StopLooking()
    {
        looking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void TriggerCameraMovement(Transform SelectedObject, bool mustMove, bool mustRotate, float distance)
    {
        //Debug.Log("Trying to move");
        this.dist = distance;
        setMovementRotation(mustMove, mustRotate);
        target = SelectedObject.position;
    }

    public void setMovementRotation(bool mustMove, bool mustRotate)
    {
        this.mustMove = mustMove;
        this.mustRotate = mustRotate;
    }

    public void MoveTowardsObject(Vector3 targetPos)
    {
        if (Vector3.Distance(transform.position, targetPos) >= dist)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, moveTowardsSpeed * Time.deltaTime);
        }
        else
        {
            mustMove = false;
        }
    }

    public void RotateTowardsObject(Vector3 targetPos)
    {
        if (Quaternion.Angle(oldRotation, Quaternion.LookRotation(targetPos - transform.position)) > 10e-1)
        {
            oldRotation = transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPos - transform.position), rotTowardsSpeed * Time.deltaTime);
            UpdateYawPitch(transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.x);
        }
        else
        {
            UpdateYawPitch(transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.x);
            mustRotate = false;
            oldRotation = new Quaternion();
        }
    }

    public void RotateAroundObject(Transform target, float distToTarget, float HorizSignal, float VertSignal)
    {
        float x = HorizSignal * freeLookSensitivity;
        float y = VertSignal * freeLookSensitivity;
        // Debug.Log(HorizSignal + " " + VertSignal);
        _rotX += x;
        _rotY += y;

        Vector3 nextRot = new Vector3(_rotX, _rotY);
        _currentRot = Vector3.SmoothDamp(_currentRot, nextRot, ref _smoothVelocity, _smoothTime);

        transform.localEulerAngles = _currentRot;
        transform.position = target.position - transform.forward * distToTarget;
    }

    public void UpdateYawPitch(float yaw, float pitch)
    {
        this.yaw = yaw;
        this.pitch = pitch;
    }
}