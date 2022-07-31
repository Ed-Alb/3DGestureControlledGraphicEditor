using UnityEngine.UI;
using UnityEngine;

public class AdaptImage : MonoBehaviour
{
    public Image stateImage;

    // Update is called once per frame
    void Update()
    {
        if (stateImage.sprite.name.Equals("Camera"))
        {
            stateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 120);
            stateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 110);
        }

        if (stateImage.sprite.name.Equals("Rotation"))
        {
            stateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 160);
            stateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
        }

        if (stateImage.sprite.name.Equals("Dragging"))
        {
            stateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 180);
            stateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 120);
        }

        if (stateImage.sprite.name.Equals("Scaling"))
        {
            stateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 150);
            stateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 150);
        }
        
        if (stateImage.sprite.name.Equals("Deformation"))
        {
            stateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 120);
            stateImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 110);
        }
    }
}
