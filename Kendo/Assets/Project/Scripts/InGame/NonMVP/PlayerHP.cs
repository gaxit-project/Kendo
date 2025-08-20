using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHP : MonoBehaviour
{
    public static PlayerHP Instance { get; private set; }

    [SerializeField] private int maxHP = 3;
    private int currentHP;
    private bool isDead = false;


    //エフェクト
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionDelay = 1.0f; // エフェクト表示時間


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
        if (isDead) return;
        currentHP--;
        Debug.Log($"くらった HP: {currentHP}");
        SoundSE.Instance?.Play("Damage");
        if (currentHP <= 0)
        {
            isDead = true;
            StartCoroutine(HandlePlayerDeath());
        }
    }



    public void KillPlayer()
    {
        currentHP = 0;
        Debug.Log($"HP: {currentHP}");
        ScoreManager.Instance?.StopTimeCount();
        GameOver();
    }

    private void GameOver()
    {
        Debug.Log("gameover");
        SoundBGM.Instance.Stop();
        ScoreManager.Instance?.SaveScoreToPlayerPrefs();
        //SceneManager.LoadScene("Result"); 
        FadeManager.Instance.FadeOutAndLoad("Result");

    }

    private IEnumerator HandlePlayerDeath()
    {
        Debug.Log("プレイヤー死亡処理開始");
        GameObject effect = null;
        Renderer effectRenderer = null;

        // スコアの時間加算を止める
        ScoreManager.Instance?.StopTimeCount();

        // プレイヤー移動停止（移動スクリプト無効化）
        var movementScript = player.Instance;
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        // 爆発エフェクト生成
        if (explosionEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 2.0f;
            Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);
            effect = Instantiate(explosionEffectPrefab, spawnPosition, rotation);
            effect.transform.localScale *= 3f;
            effectRenderer = effect.GetComponent<Renderer>();
            SetPlayerAlpha(0f);
            SoundSE.Instance?.Play("Explosion");

        }

        // 少し待ってから GameOver へ
        yield return new WaitForSeconds(explosionDelay);

        GameOver();
    }
    //Player消す
    private void SetPlayerAlpha(float alpha)
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null && rend.material.HasProperty("_Color"))
        {
            Color c = rend.material.color;
            c.a = alpha;
            rend.material.color = c;
        }
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
