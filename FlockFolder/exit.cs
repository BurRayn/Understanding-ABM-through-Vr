using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class exit : MonoBehaviour
{
    // Start is called before the first frame update
    public void QuitGame()
    {
#if UNITY_STANDALONE 
        Application.Quit();
#endif
        //If we're running in the editor, then
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    }