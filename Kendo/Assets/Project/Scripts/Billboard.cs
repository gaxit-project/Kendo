using UnityEngine;



/// <summary>
/// ��ɃJ�����̕��������I�u�W�F�N�g��]���J�����ɌŒ�
/// </summary>
public class Billboard : MonoBehaviour
{

    private void Start()
    {
        // ��]���J�����Ɠ���������
        transform.position = Camera.main.transform.position;
    }
    void LateUpdate()
    {
        // ��]���J�����Ɠ���������
        transform.rotation = Camera.main.transform.rotation;
    }
}