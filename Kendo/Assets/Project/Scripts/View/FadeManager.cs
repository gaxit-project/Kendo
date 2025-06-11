using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [SerializeField] private Image panel;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        panel.gameObject.SetActive(false);
    }

    public void FadeOutAndLoad(string sceneName)
    {
        panel.gameObject.SetActive(true);
        StartCoroutine(FadeOutCoroutine(sceneName));
    }

    private IEnumerator FadeOutCoroutine(string sceneName)
    {
        float elapsed = 0f;
        Color c = panel.color;

        // フェードアウト（透明 → 黒）
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            panel.color = c;
            yield return null;
        }

        // シーン読み込み（非同期）
        yield return SceneManager.LoadSceneAsync(sceneName);

        // フェードイン（黒 → 透明）
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            panel.color = c;
            yield return null;
        }

        // 完全に透明にして非表示
        c.a = 0f;
        panel.color = c;
        panel.gameObject.SetActive(false);
    }
}
