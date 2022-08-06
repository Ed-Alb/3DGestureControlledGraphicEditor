using System;
using System.Collections;
using UnityEngine;

public class Whiteboard : MonoBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048, 2048);
    public Texture2D oldRendererTexture;

    private GestureDetection gestDetect;
    private bool cleanTable = false;

    private InteractionType _interaction;

    void Start()
    {
        _interaction = Utilities._interaction;

        var r = GetComponent<Renderer>();
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        oldRendererTexture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        r.material.mainTexture = texture;
        Graphics.CopyTexture(texture, oldRendererTexture);

        if (_interaction == InteractionType.Kinect)
        {
            gestDetect = GameObject.Find("GestureDetectHandler").GetComponent<GestureDetection>();
            if (gestDetect != null)
            {
                gestDetect.OnGesture += ListenForWhiteboardClean;
            }
        }
    }

    private void Update()
    {
        // Clean the table by pressing R in Mouse mode or by performing
        // the close gesture (both hands on head) in the Kinect mode
        bool cleanTrigger = (_interaction == InteractionType.Mouse) ?
            Input.GetKeyDown(KeyCode.R) :
            cleanTable;

        // Clean the Whiteboard
        if (cleanTrigger)
        {
            // Debug.Log("clean");
            cleanTable = false;
            var r = this.GetComponent<Renderer>();
            Graphics.CopyTexture(oldRendererTexture, texture);
            r.material.mainTexture = texture;
        }
    }

    private void ListenForWhiteboardClean(GestureDetection.EventArgs e)
    {
        if (e.name.Contains("Close") && !cleanTable)
        {
            if (e.confidence > Utilities.CloseThreshold)
            {
                cleanTable = true;
                gestDetect.OnGesture -= ListenForWhiteboardClean;
                StartCoroutine(ReactivateWhiteboardClean());
            }
        }
    }

    private IEnumerator ReactivateWhiteboardClean()
    {
        yield return new WaitForSeconds(2);
        gestDetect.OnGesture += ListenForWhiteboardClean;
    }
}
