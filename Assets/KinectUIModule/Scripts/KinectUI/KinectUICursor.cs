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
    private bool buttonPressed = false;

    public override void Start()
    {
        base.Start();
        _initScale = transform.localScale;
        _image.color = normalColor;
    }

    public override void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Button b = collision.gameObject.GetComponent<Button>();
        if (b)
        {
            b.OnSelect(null);
            _image.color = hoverColor;
            buttonPressed = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Button b = collision.gameObject.GetComponent<Button>();
        if (b && buttonPressed)
        {
            b.onClick.Invoke();
            buttonPressed = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Button b = collision.gameObject.GetComponent<Button>();
        if (b)
        {
            b.OnDeselect(null);
            _image.color = normalColor;
            buttonPressed = true;
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
}
