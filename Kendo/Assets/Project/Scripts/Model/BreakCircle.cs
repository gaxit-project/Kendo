using UnityEngine;

public class BreakCircle : MonoBehaviour
{
    [SerializeField] private int maxHits = 3;
    [SerializeField] private Color[] hitColors;
    private int currentHits = 0;

    private Renderer rend;

    private void Awake()
    {
        // Renderer ���擾�i�e�܂��͎q�j
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            rend = GetComponentInChildren<Renderer>();
            if (rend == null)
            {
                Debug.LogWarning("Renderer ��������܂���ł���");
                return;
            }
        }

        // �}�e���A���̃C���X�^���X�𐶐����ċ��L�h�~
        rend.material = new Material(rend.material);

        // �����F�̐ݒ�
        if (hitColors != null && hitColors.Length > 0)
        {
            rend.material.color = hitColors[0];
            Debug.Log("�����F��ݒ肵�܂���: " + hitColors[0]);
        }
        else
        {
            Debug.LogWarning("hitColors ���ݒ肳��Ă��܂���");
        }
    }

    public void OnHitByKnockback()
    {
        currentHits++;
        Debug.Log($"BreakCircle: Hit {currentHits}/{maxHits}");

        if (rend != null && currentHits < hitColors.Length)
        {
            rend.material.color = hitColors[currentHits];
            Debug.Log($"BreakCircle: �F�ύX �� {hitColors[currentHits]}");
        }

        if (currentHits >= maxHits)
        {
            Debug.Log("BreakCircle: �ő�q�b�g���ɒB�����̂Ŕ�\����");
            gameObject.SetActive(false);
        }
    }
    public void OnHitRelayFromChild(Collider other)
    {
        Debug.Log($"[Relay] �qCollider�� {other.name} �ɏՓ�");

        if (other.CompareTag("Mob"))
        {
            var mob = other.GetComponent<MobController>();
            if (mob != null && mob.GetIsKnockback())
            {
                Debug.Log("�m�b�N�o�b�N����Mob�Ƀq�b�g �� �F�ύX");
                OnHitByKnockback();
            }
        }
    }

}
