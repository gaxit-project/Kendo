using UnityEngine;

public class TitleBGM : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // BGM���Đ�
        SoundBGM.Instance.Play("Title");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
