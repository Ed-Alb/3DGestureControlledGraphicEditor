using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Windows.Kinect;

public class KinectUICursor : AbstractKinectUICursor
{
    public Color normalColor = new Color(1f, 1f, 1f, 0.5f);
    public Color hoverColor = new Color(1f, 1f, 1f, 1f);
    public Color clickColor = new Color(1f, 1f, 1f, 1f);
    public Vector3 clickScale = new Vector3(.8f, .8f, .8f);

    private Vector3 _initScale;
    private bool buttonGestureDone = false;
    private bool buttonPressed = false;
    private bool enteredButton = false;

    private bool actionDone = false;

    public override void Start()
    {
        base.Start();
        _initScale = transform.localScale;
        _image.color = normalColor;
    }

    public override void Update()
    {
        KeepHandsOnScreen();
    }

    private void KeepHandsOnScreen()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, 0, Screen.width);
        pos.y = Mathf.Clamp(pos.y, 0, Screen.height);
        transform.position = pos;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Button b = collision.gameObject.GetComponent<Button>();
        if (b)
        {
            enteredButton = true;
            buttonPressed = false;
            b.OnSelect(null);
            _image.color = hoverColor;
            buttonGestureDone = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Button b = collision.gameObject.GetComponent<Button>();
        if (b && buttonPressed && buttonGestureDone && b.interactable)
        {
            b.onClick.Invoke();
            buttonPressed = false;
            buttonGestureDone = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Button b = collision.gameObject.GetComponent<Button>();
        if (b)
        {
            enteredButton = false;
            b.OnDeselect(null);
            _image.color = normalColor;
            buttonPressed = false;
            _image.color = normalColor;
        }
    }

    public override void ProcessData()
    {
        // update pos
        transform.position = _data.GetHandScreenPosition();
        Debug.Log(name + " " + transform.position);

        if (_data.IsPressing)
        {
            _image.color = clickColor;
            _image.transform.localScale = clickScale;
            return;
        }
        if (_data.IsHovering)
        {
            _image.color = hoverColor;
        }
        else
        {
            _image.color = normalColor;
        }
        _image.transform.localScale = _initScale;
    }

    public Vector3 GetHandCursorPosition()
    {
        return this.transform.position;
    }

    public Color GetNormalColor()
    {
        return normalColor;
    }

    public Color GetHoverColor()
    {
        return hoverColor;
    }

    public Color GetClickColor()
    {
        return clickColor;
    }

    public Image GetHandImg()
    {
        return _image;
    }

    public bool GetEnteredButton()
    {
        return enteredButton;
    }

    public void SetButtonPressed(bool state)
    {
        this.buttonPressed = state;
    }

    public void SetActionDone(bool state)
    {
        this.actionDone = state;
    }

    public bool IsPerformingAction()
    {
        return actionDone;
    }
}
