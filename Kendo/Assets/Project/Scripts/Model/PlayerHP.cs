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
        Debug.Log($"������� HP: {currentHP}");

        if (currentHP <= 0)
        {
            GameOver();
        }
    }

    public void KillPlayer()
    {
        currentHP = 0;
        Debug.Log($"������� HP: {currentHP}");
        GameOver();
    }

    private void GameOver()
    {
        Debug.Log("����");
        ScoreManager.Instance?.SaveScoreToPlayerPrefs();
        SceneManager.LoadScene("Result"); // �Q�[���I�[�o�[���Ƀ��U���g��
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
