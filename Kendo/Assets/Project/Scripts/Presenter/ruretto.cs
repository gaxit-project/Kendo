using UnityEngine;
using System.Collections;

public class ruretto : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 100f;
    [SerializeField] private float reverseDuration = 2f;
    private float reverseTimer = 0f;

    private bool isRotating = true;
    private bool isReverse = false;
    private bool isStoppedByBomb = false;

    void Update()
    {
        // ボムが有効になったら停止
        if (PlayerBom.bom && !isStoppedByBomb)
        {
            StopRouletteForSeconds(10f);
            return;
        }

        if (!isRotating || isStoppedByBomb)
            return;

        // 回転方向（通常／逆）
        float direction = isReverse ? -1f : 1f;
        transform.Rotate(Vector3.up * direction * rotateSpeed * Time.deltaTime);

        // 一定時間で回転方向切り替え
        reverseTimer += Time.deltaTime;
        if (reverseTimer >= reverseDuration)
        {
            isReverse = !isReverse;
            reverseTimer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mob"))
        {
            var mob = other.GetComponent<MobController>();
            if (mob != null /*&& mob.hit*/)
            {
                isRotating = false;
                Debug.Log("Mobのhitでルーレット停止");
                other.gameObject.SetActive(false);
                // スコア加算（Mobが倒されたとき）
                ScoreManager.Instance?.AddKill();
            }
        }
    }

    private void StopRouletteForSeconds(float duration)
    {
        if (isStoppedByBomb) return;
        StartCoroutine(StopRoutine(duration));
    }

    private IEnumerator StopRoutine(float duration)
    {
        isStoppedByBomb = true;
        bool wasRotatingBefore = isRotating;
        isRotating = false;

        Debug.Log("ボムでルーレット停止");

        yield return new WaitForSeconds(duration);

        PlayerBom.bom = false;      // ボム状態を解除
        isStoppedByBomb = false;
        isRotating = true;

        Debug.Log("10秒経過 → ルーレット再開");
    }
}
