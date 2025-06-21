using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    // Text を TextMeshProUGUI に変更
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI totalText;

    [SerializeField] private float timeMultiplier = 10f;
    [SerializeField] private int killMultiplier = 100;

    [SerializeField] private GameObject scorePopupObject;

    private float elapsedTime = 0f;
    private int killCount = 0;

    private bool isCountingTime = true;

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
        if (isCountingTime)
        {
            elapsedTime += Time.deltaTime;
            UpdateUI();
        }
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

        // .textプロパティの使い方は同じ
        timeText.text = $"{minutes:D2}:{seconds:D2}";
        killText.text = $"{killCount}";
        totalText.text = $"{totalScore}";
    }

    public int GetTotalScore() => Mathf.FloorToInt(elapsedTime * timeMultiplier + killCount * killMultiplier);

    //取ってくる用
    public void SaveScoreToPlayerPrefs()
    {
        PlayerPrefs.SetInt("TotalScore", GetTotalScore());
        PlayerPrefs.Save();
    }
    //スコア加算を止める
    public void StopTimeCount()
    {
        isCountingTime = false;
    }


    private IEnumerator ShowScorePopup()
    {
        if (scorePopupObject == null) yield break;

        // 初期状態の設定
        scorePopupObject.SetActive(true);

        RectTransform rect = scorePopupObject.GetComponent<RectTransform>();
        // GetComponentInChildren で TextMeshProUGUI を取得
        TextMeshProUGUI text = scorePopupObject.GetComponentInChildren<TextMeshProUGUI>(); 

        if (text == null)
        {
            Debug.LogError("TextMeshProUGUI component not found in children of scorePopupObject!");
            yield break;
        }

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, 50); // Y方向に50上へ
        Color startColor = text.color;

        float duration = 0.8f;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;

            // 上に移動
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            // フェードアウト (.colorプロパティの使い方も同じ)
            text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

            time += Time.deltaTime;
            yield return null;
        }

        // 完了時に非表示と色・位置を元に戻す
        rect.anchoredPosition = startPos;
        text.color = startColor;
        scorePopupObject.SetActive(false);
    }
}