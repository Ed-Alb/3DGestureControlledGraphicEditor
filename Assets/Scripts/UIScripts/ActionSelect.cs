using UnityEngine.UI;
using UnityEngine;
using System;

public class ActionSelect : MonoBehaviour
{
    public GameObject ScalingPanel;

    public Button rotationButton;
    public Button draggingButton;
    public Button scalingButton;
    public Button deformButton;

    public Button openPanelButton;

    private Slider XSlider;
    private Slider YSlider;
    private Slider ZSlider;

    private void Start()
    {
        // Register the button Events
        rotationButton.onClick.AddListener(() => activateRotationMode());
        draggingButton.onClick.AddListener(() => activateDraggingMode());
        scalingButton.onClick.AddListener(() => activateScalingMode());
        deformButton.onClick.AddListener(() => activateDeformingMode());

        openPanelButton.onClick.AddListener(() => OpenCloseActionSelection());
    }

    public void OpenCloseActionSelection()
    {
        ActivatePanel();
    }

    private void activateRotationMode()
    {
        Camera cam = Camera.main;
        string selectedObj = cam.GetComponent<SelectObject>().GetSelectedObjectName();
        //Debug.Log("Rotate");
        if (selectedObj != null)
        {
            HideScalingPanel(true);
            GameObject.Find(selectedObj).GetComponent<ObjectInteraction>().SetAction(actionType.Rotate);
        }
    }

    private void activateDraggingMode()
    {
        Camera cam = Camera.main;
        string selectedObj = cam.GetComponent<SelectObject>().GetSelectedObjectName();
        //Debug.Log("Drag");
        if (selectedObj != null)
        {
            HideScalingPanel(true);
            GameObject.Find(selectedObj).GetComponent<ObjectInteraction>().SetAction(actionType.Drag);
        }
    }

    private void activateScalingMode()
    {
        Camera cam = Camera.main;
        string selectedObj = cam.GetComponent<SelectObject>().GetSelectedObjectName();
        //Debug.Log("Scale");
        if (selectedObj != null)
        {
            HideScalingPanel(false);
            InitializeSliders(selectedObj);
            GameObject.Find(selectedObj).GetComponent<ObjectInteraction>().SetAction(actionType.Scale);
        }
    }

    private void activateDeformingMode()
    {
        Camera cam = Camera.main;
        string selectedObj = cam.GetComponent<SelectObject>().GetSelectedObjectName();
        //Debug.Log("Deform");
        if (selectedObj != null)
        {
            HideScalingPanel(true);
            GameObject.Find(selectedObj).GetComponent<ObjectInteraction>().SetAction(actionType.Deform);
        }
    }

    private void InitializeSliders(string objName)
    {
        XSlider = GameObject.Find("X-Axis").GetComponent<Slider>();
        YSlider = GameObject.Find("Y-Axis").GetComponent<Slider>();
        ZSlider = GameObject.Find("Z-Axis").GetComponent<Slider>();
        XSlider.SetValueWithoutNotify(GameObject.Find(objName).GetComponent<ObjectInteraction>().GetScale()[0]);
        YSlider.SetValueWithoutNotify(GameObject.Find(objName).GetComponent<ObjectInteraction>().GetScale()[1]);
        ZSlider.SetValueWithoutNotify(GameObject.Find(objName).GetComponent<ObjectInteraction>().GetScale()[2]);
    }

    public void ActivatePanel()
    {
        Animator animator = this.GetComponent<Animator>();
        //Debug.Log("open/close");
        if (animator != null)
        {
            bool isOpen = animator.GetBool("Open");
            animator.SetBool("Open", !isOpen);
        }
    }

    private void HideScalingPanel(bool hide)
    {
        if (hide)
        {
            if (ScalingPanel != null)
            {
                ScalingPanel.gameObject.SetActive(false);
            }
        }
        else
        {
            if (ScalingPanel != null)
            {
                ScalingPanel.gameObject.SetActive(true);
            }
        }
    }
}
