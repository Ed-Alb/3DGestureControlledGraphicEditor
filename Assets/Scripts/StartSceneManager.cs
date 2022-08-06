using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    private string _LoadSceneName = "EditorScene";

    public void LoadEditorScene(string interactionType)
    {
        if (interactionType.Equals("Mouse"))
        {
            Utilities.SetInteractionType(InteractionType.Mouse);
        }
        else if (interactionType.Equals("Kinect"))
        {
            Utilities.SetInteractionType(InteractionType.Kinect);
        }

        SceneManager.LoadScene(_LoadSceneName);
    }
}
