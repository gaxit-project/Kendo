using UnityEngine;

public class BreakCircle : MonoBehaviour
{
    [SerializeField] private int maxHits = 3;
    [SerializeField] private Color[] hitColors;
    private int currentHits = 0;

    private Renderer rend;

    private void Awake()
    {
        // Renderer を取得（親または子）
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            rend = GetComponentInChildren<Renderer>();
            if (rend == null)
            {
                Debug.LogWarning("Renderer が見つかりませんでした");
                return;
            }
        }

        // マテリアルのインスタンスを生成して共有防止
        rend.material = new Material(rend.material);

        // 初期色の設定
        if (hitColors != null && hitColors.Length > 0)
        {
            rend.material.color = hitColors[0];
            Debug.Log("初期色を設定しました: " + hitColors[0]);
        }
        else
        {
            Debug.LogWarning("hitColors が設定されていません");
        }
    }

    public void OnHitByKnockback()
    {
        currentHits++;
        Debug.Log($"BreakCircle: Hit {currentHits}/{maxHits}");

        if (rend != null && currentHits < hitColors.Length)
        {
            rend.material.color = hitColors[currentHits];
            Debug.Log($"BreakCircle: 色変更 → {hitColors[currentHits]}");
        }

        if (currentHits >= maxHits)
        {
            Debug.Log("BreakCircle: 最大ヒット数に達したので非表示化");
            gameObject.SetActive(false);
        }
    }
    public void OnHitRelayFromChild(Collider other)
    {
        Debug.Log($"[Relay] 子Colliderが {other.name} に衝突");

        if (other.CompareTag("Mob"))
        {
            var mob = other.GetComponent<MobController>();
            if (mob != null && mob.GetIsKnockback())
            {
                Debug.Log("ノックバック中のMobにヒット → 色変更");
                OnHitByKnockback();
            }
        }
    }

}
