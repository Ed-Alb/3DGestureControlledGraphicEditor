using UnityEngine;
using UnityEngine.UI;

public class ButtonsScroll : MonoBehaviour
{
    private InteractionType _interaction;
    [SerializeField] private Button UpScroll;
    [SerializeField] private Button DownScroll;
    [SerializeField] private ScrollRect ObjectsListScrollRect;

    private Transform LeftHandTransf;
    private float scrollSpeed = 0.5f;

    void Start()
    {
        _interaction = Utilities._interaction;

        if(_interaction == InteractionType.Mouse)
        {
            UpScroll.interactable = false;
            DownScroll.interactable = false;
        }
    }

    private void Update()
    {
        HandleScrollButtons(Time.deltaTime);
    }

    void HandleScrollButtons(float deltaTime)
    {
        if (_interaction == InteractionType.Kinect)
        {
            GameObject LeftHand = GameObject.Find("Left Hand");
            if (LeftHand)
            {
                KinectUICursor LeftHandCursor = LeftHand.GetComponent<KinectUICursor>();
                bool downButtonTrigg = LeftHandCursor.downScroll;
                bool upButtonTrigg = LeftHandCursor.upScroll;

                if (ObjectsListScrollRect)
                {
                    if (downButtonTrigg)
                    {
                        //Debug.Log("Scroll Down");
                        ObjectsListScrollRect.verticalNormalizedPosition -= scrollSpeed * deltaTime;
                    }
                    else if (upButtonTrigg)
                    {
                        //Debug.Log("Scroll Up");
                        ObjectsListScrollRect.verticalNormalizedPosition += scrollSpeed * deltaTime;
                    }
                }
            }
        }
    }
}
