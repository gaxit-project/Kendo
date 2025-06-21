#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ExitController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void onClickExitButton()
    {
        SoundSE.Instance?.Play("Cancel");
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

}
