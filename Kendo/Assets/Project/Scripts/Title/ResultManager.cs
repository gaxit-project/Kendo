using UnityEngine;
using TMPro;

public class ResultManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private float countUpDuration = 3f; // �J�E���g�A�b�v�ɂ����鎞�ԁi�b�j

    private int targetScore;
    private float currentDisplayScore = 0f;
    private float timer = 0f;

    void Start()
    {
        targetScore = PlayerPrefs.GetInt("TotalScore", 0);
        currentDisplayScore = 0f;
        scoreText.text = "Score : 0";
        SoundSE.Instance?.Play("Count");
    }

    void Update()
    {
        if (currentDisplayScore < targetScore)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / countUpDuration);
            currentDisplayScore = Mathf.Lerp(0, targetScore, progress);

            int displayInt = Mathf.FloorToInt(currentDisplayScore);
            string scoreStr = displayInt == 0 ? "" : displayInt.ToString();

            int totalWidth = 6; // �ő包��
            string paddedScore = scoreStr.PadLeft(totalWidth, ' '); // ���l��

            scoreText.text = $"Score : {paddedScore}";
        }
    }


}
