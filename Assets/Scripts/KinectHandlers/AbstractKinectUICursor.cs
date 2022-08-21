using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Windows.Kinect;
/// <summary>
/// Abstract UI component class for hand cursor objects
/// </summary>
[RequireComponent(typeof(CanvasGroup), typeof(Image))]
public abstract class AbstractKinectUICursor : MonoBehaviour
{
    protected Image _image;

    public virtual void Start()
    {
        Setup();
    }

    protected void Setup()
    {
        // Make sure we dont block raycasts
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        GetComponent<CanvasGroup>().interactable = false;
        // image component of the hand
        _image = GetComponent<Image>();
    }

    public virtual void Update()
    {
    }
}