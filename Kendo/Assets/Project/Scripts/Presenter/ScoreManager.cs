using UnityEngine;
using UnityEngine.UI;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private Text timeText;
    [SerializeField] private Text killText;
    [SerializeField] private Text totalText;

    [SerializeField] private float timeMultiplier = 10f;
    [SerializeField] private int killMultiplier = 100;

    private float elapsedTime = 0f;
    private int killCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        UpdateUI();
    }

    public void AddKill()
    {
        killCount++;
    }

    private void UpdateUI()
    {
        int totalScore = Mathf.FloorToInt(elapsedTime * timeMultiplier + killCount * killMultiplier);
        int totalSeconds = Mathf.FloorToInt(elapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timeText.text = $"{minutes:D2}:{seconds:D2}";
        killText.text = $"{killCount}";
        totalText.text = $"{totalScore}";
    }

    public int GetTotalScore() => Mathf.FloorToInt(elapsedTime * timeMultiplier + killCount * killMultiplier);

    //Žæ‚Á‚Ä‚­‚é—p
    public void SaveScoreToPlayerPrefs()
    {
        PlayerPrefs.SetInt("TotalScore", GetTotalScore());
        PlayerPrefs.Save();
    }

}
