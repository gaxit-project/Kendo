using UnityEngine;

public class ConfigBotton : MonoBehaviour
{
    [SerializeField] private GameObject configCanvas; // �\�����������ݒ�Canvas

    public void OnClickOpenConfig()
    {
        configCanvas.SetActive(true); // �\���ɂ���
    }
    public void OnClickCloseConfig()
    {
        configCanvas.SetActive(false); // ��\���ɂ���
    }

}
