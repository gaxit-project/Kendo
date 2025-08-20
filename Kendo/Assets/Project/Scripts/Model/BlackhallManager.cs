using UnityEngine;
using System.Collections;

public class BlackhallManager : MonoBehaviour
{
    [Header("SuckOption")]
    [SerializeField] private string mobTag = "Mob";
    [SerializeField] private string tacklemobTag = "TackleMob";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float suckDuration = 1.5f; // 吸い込みにかかる時間


    [Header("ExpandOption")]
    [SerializeField] private Vector3 initialScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private float expandRatePerSecond = 0.1f;  // 時間ごとの拡大率
    [SerializeField] private float expandRatePerMinute = 0.1f;  // 1分ごとの拡大率の拡大率
    [SerializeField] private float shrinkFactor = 0.9f;         // 吸い込みごとの縮小率（例：90%）
    [SerializeField] private CircleManager circleManager;
    private float CntTime = 0f;


    private void Awake()
    {
        initialScale = transform.localScale;
    }

    private void Start()
    {
        transform.localScale = initialScale;
    }

    private void Update()
    {
        CntTime += Time.deltaTime;

        if (CntTime > 60f)
        {
            expandRatePerSecond += expandRatePerMinute;
            CntTime = 0f;
        }

        // 毎秒少しずつ大きくする
        Vector3 scale = transform.localScale;
        scale.x += expandRatePerSecond * Time.deltaTime;
        scale.z += expandRatePerSecond * Time.deltaTime;
        scale.y = initialScale.y;
        transform.localScale = scale;

        // スケールの下限（最小値はinitialScaleすなわち初期値）
        transform.localScale = new Vector3(
        Mathf.Max(transform.localScale.x, initialScale.x),
        initialScale.y,
        Mathf.Max(transform.localScale.z, initialScale.z)
        );

        // CircleManager にスケールを送る
        circleManager?.UpdateCircleScale(transform.localScale);
    }


    private void OnTriggerEnter(Collider other)
    {
        // TackleMobの場合はノックバックなしでもガチャ発動&縮小あり
        if (other.CompareTag(tacklemobTag))
        {
            // 吸い込み＋破壊＋ガチャ＋縮小
            StartCoroutine(SuckAndDestroy(other.gameObject));
        }
        if (other.CompareTag(mobTag))
        {
            var mobController = other.GetComponent<MobController>();
            if (mobController != null && mobController.GetIsKnockback())
            {
                // ノックバック中 → 吸い込み＋破壊＋ガチャ＋縮小
                StartCoroutine(SuckAndDestroy(other.gameObject));
            }
            else
            {
                // ノックバック中でない → 吸い込み＋破壊のみ
                StartCoroutine(SuckAndDestroyOnly(other.gameObject));
            }
        }
        else if (other.CompareTag(playerTag))
        {
            StartCoroutine(SuckAndKillPlayer(other.gameObject));
        }
    }
    private IEnumerator SuckAndDestroy(GameObject mob)
    {
        SoundSE.Instance?.Play("Blackhall");

        // Rigidbodyや移動を無効化
        Rigidbody rb = mob.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // 開始位置と時間の記録
        Vector3 startPos = mob.transform.position;
        Vector3 endPos = transform.position; // ブラックホール中心
        Vector3 startScale = mob.transform.localScale;
        Vector3 endScale = startScale * 0.5f; // 最終的に0.5倍に
        float timer = 0f;
        float rotationSpeed = 180f; // 1秒で360度


        // 吸い込まれるように徐々に移動
        while (timer < suckDuration)
        {
            if (mob == null) yield break;

            timer += Time.deltaTime;
            float t = timer / suckDuration;

            mob.transform.position = Vector3.Lerp(startPos, endPos, t);
            mob.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            mob.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // 最後に破壊＋ガチャ処ri
        MobManager.Instance.ReleaseMob(mob);
        GachaManager.Instance.Gacha();
        
        // ブラックホールを少し縮小
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Max(newScale.x * shrinkFactor, initialScale.x);
        newScale.z = Mathf.Max(newScale.z * shrinkFactor, initialScale.z);
        newScale.y = initialScale.y; 
        transform.localScale = newScale;
        //障害物に送る
        circleManager?.UpdateCircleScale(newScale);
    }

    //ノックバック中じゃないときは吸い込まれて破壊するだけ
    private IEnumerator SuckAndDestroyOnly(GameObject mob)
    {
        SoundSE.Instance?.Play("Blackhall");

        Rigidbody rb = mob.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Vector3 startPos = mob.transform.position;
        Vector3 endPos = transform.position;
        Vector3 startScale = mob.transform.localScale;
        Vector3 endScale = startScale * 0.5f;
        float timer = 0f;
        float rotationSpeed = 180f;

        while (timer < suckDuration)
        {
            if (mob == null) yield break;

            timer += Time.deltaTime;
            float t = timer / suckDuration;

            mob.transform.position = Vector3.Lerp(startPos, endPos, t);
            mob.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            mob.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // 破壊のみ（ガチャや縮小なし）
        MobManager.Instance.ReleaseMobWithoutScore(mob);
    }

    private IEnumerator SuckAndKillPlayer(GameObject player)
    {
        SoundSE.Instance?.Play("Blackhall");

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // PlayerMovementやコントロールスクリプトを停止
        MonoBehaviour[] components = player.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            comp.enabled = false;
        }

        Vector3 startPos = player.transform.position;
        Vector3 endPos = transform.position;
        Vector3 startScale = player.transform.localScale;
        Vector3 endScale = startScale * 0.5f;
        float timer = 0f;
        float rotationSpeed = 180f;

        while (timer < suckDuration)
        {
            if (player == null) yield break;

            timer += Time.deltaTime;
            float t = timer / suckDuration;

            player.transform.position = Vector3.Lerp(startPos, endPos, t);
            player.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            player.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // プレイヤー死亡処理
        PlayerHP.Instance.KillPlayer(); // GameOver画面などに移行
    }
}
