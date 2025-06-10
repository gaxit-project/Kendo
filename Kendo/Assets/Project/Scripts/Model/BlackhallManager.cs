using UnityEngine;
using System.Collections;

public class BlackhallManager : MonoBehaviour
{
    [SerializeField] private string mobTag = "Mob";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float suckDuration = 1.5f; // 吸い込みにかかる時間

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
            PlayerHP.Instance.KillPlayer();

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
    }
}
