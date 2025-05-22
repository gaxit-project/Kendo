using UnityEngine;

public class TitleBGM : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // BGM‚ğÄ¶
        SoundBGM.Instance.Play("Title");

    }

    private void OnDestroy()
    {
        // ƒV[ƒ“Ø‚è‘Ö‚¦‚ÅBGM‚ğ~‚ß‚é
        SoundBGM.Instance.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
