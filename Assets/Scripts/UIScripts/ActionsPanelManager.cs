using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionsPanelManager : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject AnimatedPanel;
    private bool needToClose = false;
    private bool selected = false;

    // Referencing of all the buttons so that I can make them interactable or not
    [SerializeField] private Button _rotateButton;
    [SerializeField] private Button _dragButton;
    [SerializeField] private Button _scaleButton;
    [SerializeField] private Button _deformButton;

    // Update is called once per frame
    void Update()
    {
        Camera cam = Camera.main;
        string selectedObj = cam.GetComponent<SelectObject>().GetSelectedObjectName();
        // Daca s-a selectat un obiect, se deschide Panoul
        if (selectedObj != null && !selected)
        {
            selected = true;
            ActivatePanel(true);
        }

        // Start Close Process
        if (selectedObj == null && selected)
        {
            ActivatePanel(false);
            selected = false;
        }

        if (needToClose)
        {
            ClosePanel();
        }
    }

    // Activate or Deactivate Panel and play its Animation
    private void ActivatePanel(bool act)
    {
        if (act)
        {
            needToClose = false;
            MakeButtonsInteractable(true);
            MainPanel.SetActive(act);
            PlayPanelAnimation(true);
        }
        else
        {
            needToClose = true;
            MakeButtonsInteractable(false);
            PlayPanelAnimation(false);
        }
    }

    // Play animation forward (direction = true) or backward (direction = false)
    private void PlayPanelAnimation(bool direction)
    {
        if (AnimatedPanel != null)
        {
            Animator animator = AnimatedPanel.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("Open", direction);
            }
        }
    }

    // If user exited object selection, then wait for panel's animation
    // to finish and then "close" the panel
    private void ClosePanel()
    {
        if (AnimatedPanel != null)
        {
            Animator animator = AnimatedPanel.GetComponent<Animator>();
            if (animator != null)
            {
                if (animator.GetCurrentAnimatorStateInfo(0).length == 1)
                {
                    MainPanel.SetActive(false);
                    needToClose = false;
                }
            }
        }
    }

    private void MakeButtonsInteractable(bool state)
    {
        _rotateButton.interactable = state;
        _dragButton.interactable = state;
        _scaleButton.interactable = state;
        _deformButton.interactable = state;
    }
}
