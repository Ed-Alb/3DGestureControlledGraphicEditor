using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinectHandsEvents : MonoBehaviour
{
    private GameObject LeftHand;
    private GameObject RightHand;

    private GestureDetection gestDetect;

    private TMPro.TextMeshProUGUI _gestureText;

    private bool RActionDone = false;
    private bool LActionDone = false;
    private bool closeActionDone = false;
    private bool TwoFingersActionDone = false;
    private bool WhiteboardActivated = false;

    void Start()
    {
        _gestureText = GameObject.Find("Gesture (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        gestDetect = GameObject.Find("GestureDetectHandler").GetComponent<GestureDetection>();
        if (gestDetect != null)
        {
            gestDetect.OnGesture += ListenForGesture;
        }
    }

    void Update()
    {
        LeftHand = GameObject.Find("Left Hand");
        RightHand = GameObject.Find("Right Hand");

        if (RActionDone && LActionDone)
        {
            _gestureText.text = "Both Hands";
        }
        else if (RActionDone)
        {
            _gestureText.text = "Right Hand";
        }
        else if (LActionDone)
        {
            _gestureText.text = "Left Hand";
        }
        else if (closeActionDone)
        {
            _gestureText.text = "Close";
        }
        else if (TwoFingersActionDone)
        {
            _gestureText.text = "Two Fingers";
        }
        else if (WhiteboardActivated)
        {
            _gestureText.text = "Whiteboard";
        }
        else if (!RActionDone && !LActionDone && !closeActionDone)
        {
            _gestureText.text = "Normal";
        }
    }

    private void ListenForGesture(GestureDetection.EventArgs e)
    {
        //Debug.Log("Truly detected gesture " + e.name + " with confidence of " + e.confidence);
        if (e.name.Contains("Left") && LeftHand)
        {
            CheckHandAction(LeftHand, e.confidence, e.name, Utilities.LeftHandThreshold, "left");
        }
        else if (e.name.Contains("Right") && RightHand)
        {
            CheckHandAction(RightHand, e.confidence, e.name, Utilities.RightHandThreshold, "right");
        }
        else if (e.name.Contains("Close"))
        {
            CheckCloseAction(e.confidence, e.name, Utilities.CloseThreshold);
        }
        else if (e.name.Contains("Two"))
        {
            CheckTwoFingersAction(e.confidence, e.name, Utilities.TwoFingersThreshold);
        }
        else if (e.name.Contains("ActWhiteboard") || e.name.Contains("Terrain"))
        {
            CheckWhiteboardActivation(e.confidence, e.name, Utilities.WhiteboardThreshold);
        }
    }

    private void CheckWhiteboardActivation(float confidence, string action_name, float conf_threshold)
    {
        if (confidence > conf_threshold)
        {
            WhiteboardActivated = true;
        }
        else
        {
            WhiteboardActivated = false;
        }
    }

    private void CheckTwoFingersAction(float confidence, string action_name, float conf_threshold)
    {
        if (confidence > conf_threshold)
        {
            TwoFingersActionDone = true;
        }
        else
        {
            TwoFingersActionDone = false;
        }
    }

    private void CheckCloseAction(float confidence, string action_name, float conf_threshold)
    {
        if (confidence > conf_threshold)
        {
            closeActionDone = true;
        }
        else
        {
            closeActionDone = false;
        }
    }

    private void CheckHandAction(GameObject Hand, float confidence, string action_name, float conf_threshold, string handType)
    {
        if (confidence > conf_threshold)
        {
            Hand.GetComponent<KinectUICursor>().GetHandImg().color = Hand.GetComponent<KinectUICursor>().GetClickColor();
            Hand.GetComponent<KinectUICursor>().SetButtonPressed(true);

            bool state = false;
            if (handType.Equals("right"))
            {
                RActionDone = true;
                state = RActionDone;
            }
            else if (handType.Equals("left"))
            {
                LActionDone = true;
                state = LActionDone;
            }
            Hand.GetComponent<KinectUICursor>().SetActionDone(state);
        }
        else
        {
            if (!Hand.GetComponent<KinectUICursor>().GetEnteredButton())
            {
                Hand.GetComponent<KinectUICursor>().GetHandImg().color = Hand.GetComponent<KinectUICursor>().GetNormalColor();
            }

            bool state = false;
            if (handType.Equals("right"))
            {
                RActionDone = false;
                state = RActionDone;
            }
            else if (handType.Equals("left"))
            {
                LActionDone = false;
                state = LActionDone;
            }
            Hand.GetComponent<KinectUICursor>().SetActionDone(state);
        }
    }

    public bool GetRAction()
    {
        return this.RActionDone;
    }

    public bool GetLAction()
    {
        return this.LActionDone;
    }

    public bool GetCloseAction()
    {
        return this.closeActionDone;
    }

    public bool GetWhiteboardAction()
    {
        return this.WhiteboardActivated;
    }

    public bool GetTwoFingersAction()
    {
        return this.TwoFingersActionDone;
    }
}
