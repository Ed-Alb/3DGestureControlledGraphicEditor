using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DisplayState : MonoBehaviour
{
    // Variables for State Representation
    private Image ModeImage;
    private Sprite[] spriteArray;
    private Text stateText;

    public Dictionary<string, Sprite> sprites =
    new Dictionary<string, Sprite>();


    void Start()
    {
        ModeImage = GameObject.Find("Mode").GetComponent<Image>();
        spriteArray = Resources.LoadAll<Sprite>("Images");
        stateText = GameObject.Find("StateText").GetComponent<Text>();
        for (int i = 0; i < spriteArray.Length; i++)
        {
            sprites.Add(spriteArray[i].name, spriteArray[i]);
        }
    }

    void Update()
    {
        setStateUI();
    }

    // Modifies Image and Text in bottom right
    // so that the user knows what action he is performing
    private void setStateUI()
    {
        Camera cam = Camera.main;
        string selectedObj = cam.GetComponent<SelectObject>().GetSelectedObjectName();
        if (selectedObj != null)
        {
            ObjectInteraction obj = GameObject.Find(selectedObj).GetComponent<ObjectInteraction>();
            switch (obj.action)
            {
                case actionType.Drag:
                    setTextAndState("Dragging", "Move/Drag");
                    break;

                case actionType.Rotate:
                    setTextAndState("Rotation", "Rotate");
                    break;

                case actionType.Scale:
                    setTextAndState("Scaling", "Scaling");
                    break;

                case actionType.Deform:
                    setTextAndState("Deformation", "Deformation");
                    break;
            }
        }
        else
        {
            if (!WhiteboardHandler._whiteboardActive)
            {
                setTextAndState("Camera", "Free Look");
            }
            else
            {
                WhiteboardHandler wh = FindObjectOfType<WhiteboardHandler>();
                if (wh)
                {
                    if (wh.GetWhiteboardActive() == WhiteboardType.Sketch)
                    {
                        setTextAndState("Whiteboard", "Whiteboard");
                    }
                    else if (wh.GetWhiteboardActive() == WhiteboardType.Terrain)
                    {
                        setTextAndState("Map", "Terr Builder");
                    }
                }
            }
        }
    }

    private void setTextAndState(string imageName, string stateName)
    {
        ModeImage.sprite = sprites[imageName];
        stateText.text = stateName;
    }
}
