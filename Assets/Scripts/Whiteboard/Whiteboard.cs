using UnityEngine;

public class Whiteboard : MonoBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048, 2048);
    public Texture2D oldRendererTexture;

    void Start()
    {
        var r = GetComponent<Renderer>();
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        oldRendererTexture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        r.material.mainTexture = texture;
        Graphics.CopyTexture(texture, oldRendererTexture);
    }

    private void Update()
    {
        // Clean the Whiteboard
        if (Input.GetKeyDown(KeyCode.R))
        {
            var r = this.GetComponent<Renderer>();
            Graphics.CopyTexture(oldRendererTexture, texture);
            r.material.mainTexture = texture;
        }
    }
}
