using UnityEngine;

public class TitleBGM : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // BGM���Đ�
        SoundBGM.Instance.Play("Title");

    }

    private void OnDestroy()
    {
        // �V�[���؂�ւ���BGM���~�߂�
        SoundBGM.Instance.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
