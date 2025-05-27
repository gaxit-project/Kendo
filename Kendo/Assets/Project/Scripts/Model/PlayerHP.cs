using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHP : MonoBehaviour
{
    public static PlayerHP Instance { get; private set; }

    [SerializeField] private int maxHP = 3;
    private int currentHP;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentHP = maxHP;
    }

    public void TakeDamage()
    {
        currentHP--;
        Debug.Log($"くらった HP: {currentHP}");

        if (currentHP <= 0)
        {
            GameOver();
        }
    }

    public void KillPlayer()
    {
        currentHP = 0;
        Debug.Log($"くらった HP: {currentHP}");
        GameOver();
    }

    private void GameOver()
    {
        Debug.Log("がめ");
        ScoreManager.Instance?.SaveScoreToPlayerPrefs();
        SceneManager.LoadScene("Result"); // ゲームオーバー時にリザルトへ
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }

    public void RecoverHP()
    {
        if (currentHP <= 2)
        {
            currentHP++;
        }
    }
}
