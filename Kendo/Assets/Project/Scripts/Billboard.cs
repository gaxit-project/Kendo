using UnityEngine;



/// <summary>
/// ��ɃJ�����̕��������I�u�W�F�N�g��]���J�����ɌŒ�
/// </summary>
public class Billboard : MonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField]
    private float offset;

    private void Awake()
    {
        // ��]���J�����Ɠ���������
        rectTransform = GetComponent<RectTransform>();
    }
    
    void LateUpdate()
    {
        // ��]���J�����Ɠ���������
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y - offset, Camera.main.transform.position.z);
        transform.rotation = Camera.main.transform.rotation;

        // Orthographic Size�����Canvas�T�C�Y���v�Z
        float height = Camera.main.orthographicSize * 2f;
        float width = height * Camera.main.aspect;

        // RectTransform�̃T�C�Y��ݒ�
        rectTransform.sizeDelta = new Vector2(width, height);
    }
}