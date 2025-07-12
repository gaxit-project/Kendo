using UnityEngine;

public class BreakableObstacle : MonoBehaviour
{
    [SerializeField] private int maxHits = 2;
    [SerializeField] private GameObject[] stages; // ƒqƒbƒg‚²‚Æ‚É•\¦‚·‚é GameObject ‚ğŠi”[i—á: 2ŒÂj

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
            gameObject.SetActive(false); // áŠQ•¨‚²‚ÆÁ‚¦‚é
        }
        else
        {
            UpdateStageAppearance(); // Ÿ‚ÌŒ©‚½–Ú‚ÉØ‚è‘Ö‚¦
        }
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
