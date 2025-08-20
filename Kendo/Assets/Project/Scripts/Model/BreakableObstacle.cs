using UnityEngine;

public class BreakableObstacle : MonoBehaviour
{
    [SerializeField] private int maxHits = 2;
    [SerializeField] private GameObject[] stages; // �q�b�g���Ƃɕ\������ GameObject ���i�[�i��: 2�j

    private int currentHits = 0;

    private void Start()
    {
        UpdateStageAppearance();
    }

    public void Hit()
    {
        currentHits++;
        Debug.Log($"BreakableObstacle hit! count: {currentHits}");

        if (currentHits >= maxHits)
        {
            Debug.Log("BreakableObstacle destroyed.");
            gameObject.SetActive(false); // ��Q�����Ə�����
        }
        else
        {
            UpdateStageAppearance(); // ���̌����ڂɐ؂�ւ�
        }
    }

    //�O����Ăяo���ĉ�
    public void ForceBreak()
    {
        currentHits = maxHits;
        UpdateStageAppearance();
        gameObject.SetActive(false);
        Debug.Log("777�ŕǉ�");
    }


    private void UpdateStageAppearance()
    {
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i] != null)
                stages[i].SetActive(i == currentHits);
        }
    }
}
