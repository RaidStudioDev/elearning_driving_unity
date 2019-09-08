using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

// suppress warning CS0618: `This component is part of the legacy UI system and will be removed in a future release.'
#pragma warning disable 618
[RequireComponent(typeof (GUITexture))]
#pragma warning restore 618
public class ForcedReset : MonoBehaviour
{
    private void Update()
    {
        // if we have forced a reset ...
        if (CrossPlatformInputManager.GetButtonDown("ResetObject"))
        {
            //... reload the scene
            SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
        }
    }
}
