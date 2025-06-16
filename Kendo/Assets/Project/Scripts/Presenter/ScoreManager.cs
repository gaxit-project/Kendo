using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private Text timeText;
    [SerializeField] private Text killText;
    [SerializeField] private Text totalText;

    [SerializeField] private float timeMultiplier = 10f;
    [SerializeField] private int killMultiplier = 100;

    [SerializeField] private GameObject scorePopupObject;

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
        StartCoroutine(ShowScorePopup());
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

    //����Ă���p
    public void SaveScoreToPlayerPrefs()
    {
        PlayerPrefs.SetInt("TotalScore", GetTotalScore());
        PlayerPrefs.Save();
    }
    private IEnumerator ShowScorePopup()
    {
        if (scorePopupObject == null) yield break;

        // ������Ԃ̐ݒ�
        scorePopupObject.SetActive(true);

        RectTransform rect = scorePopupObject.GetComponent<RectTransform>();
        Text text = scorePopupObject.GetComponentInChildren<Text>(); 

        if (text == null)
        {
            Debug.LogError("Text component not found!");
            yield break;
        }

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, 50); // Y������50���
        Color startColor = text.color;

        float duration = 0.8f;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;

            // ��Ɉړ�
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            // �t�F�[�h�A�E�g
            text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

            time += Time.deltaTime;
            yield return null;
        }

        // �������ɔ�\���ƐF�E�ʒu�����ɖ߂�
        rect.anchoredPosition = startPos;
        text.color = startColor;
        scorePopupObject.SetActive(false);
    }


}
