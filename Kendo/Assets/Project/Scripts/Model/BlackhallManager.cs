using UnityEngine;
using System.Collections;

public class BlackhallManager : MonoBehaviour
{
    [Header("SuckOption")]
    [SerializeField] private string mobTag = "Mob";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float suckDuration = 1.5f; // 吸い込みにかかる時間

    [Header("ExpandOption")]
    [SerializeField] private CircleManager circleManager;
    [SerializeField] private Vector3 initialScale;
    [SerializeField] private int DestroyedEnemy;
    [SerializeField] private int NumExpand = 1; // 一定数で拡大
    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private GameObject barrier;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    private void Start()
    {
        transform.localScale = initialScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(mobTag))
        {
            //Destroy(other.gameObject);
            //GachaManager.Instance.Gacha();
            StartCoroutine(SuckAndDestroy(other.gameObject));
        }
        else if (other.CompareTag(playerTag))
        {
            //PlayerHP.Instance.KillPlayer();
            StartCoroutine(SuckAndKillPlayer(other.gameObject));

        }
    }
    private IEnumerator SuckAndDestroy(GameObject mob)
    {
        SoundSE.Instance?.Play("warp");

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
        Destroy(mob);
        GachaManager.Instance.Gacha();
        DestroyedEnemy++;

        if (DestroyedEnemy == NumExpand)
        {
            ExpandBlackHoleAndBarrier();
            DestroyedEnemy = 0;
        }
    }

    private IEnumerator SuckAndKillPlayer(GameObject player)
    {
        SoundSE.Instance?.Play("warp");

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

    private void ExpandBlackHoleAndBarrier()
    {
        transform.localScale *= scaleMultiplier;

        if (barrier != null)
        {
            barrier.transform.localScale *= scaleMultiplier;

            // Colliderのサイズ調整
            SphereCollider sc = barrier.GetComponent<SphereCollider>();
            if (sc != null)
            {
                sc.radius *= scaleMultiplier;
            }
        }

        // CircleManager側の障壁オブジェクト群も拡大
        if (circleManager != null)
        {
            circleManager.ExpandCircle(scaleMultiplier);
        }
    }
}
