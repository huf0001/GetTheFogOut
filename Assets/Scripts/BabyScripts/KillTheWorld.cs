using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KillTheWorld : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        RestartCheck();
    }
    
    private void RestartCheck()
    {
        if (Keyboard.current[Key.F5].wasReleasedThisFrame)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe")); 
            Application.Quit();
#endif
        }
    }
}
