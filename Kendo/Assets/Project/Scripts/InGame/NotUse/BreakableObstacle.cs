using UnityEngine;

public class BreakableObstacle : MonoBehaviour
{
    [SerializeField] private int maxHits = 2;
    [SerializeField] private GameObject[] stages; // ヒットごとに表示する GameObject を格納（例: 2個）

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
            gameObject.SetActive(false); // 障害物ごと消える
        }
        else
        {
            UpdateStageAppearance(); // 次の見た目に切り替え
        }
    }

    //外から呼び出して壊す
    public void ForceBreak()
    {
        currentHits = maxHits;
        UpdateStageAppearance();
        gameObject.SetActive(false);
        Debug.Log("777で壁壊す");
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
