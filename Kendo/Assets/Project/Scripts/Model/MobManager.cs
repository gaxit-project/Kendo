using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobManager : MonoBehaviour
{
    public static MobManager Instance { get; private set; }

    [SerializeField] private GameObject mobPrefab;
    [SerializeField] private int poolSize = 100;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private Transform playerTransform;

    private Queue<GameObject> mobPool = new Queue<GameObject>();
    private List<GameObject> activeMobs = new List<GameObject>();

    private float minX, maxX, minZ, maxZ;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject mob = Instantiate(mobPrefab);
            mob.SetActive(false);
            mobPool.Enqueue(mob);
        }
    }

    private void Start()
    {
        // MapOutLineから座標範囲を取得
        MapOutLine outline = FindObjectOfType<MapOutLine>();
        if (outline != null)
        {
            Vector3 up = outline.MapUp.transform.position;
            Vector3 down = outline.MapDown.transform.position;
            Vector3 left = outline.MapLeft.transform.position;
            Vector3 right = outline.MapRight.transform.position;

            minX = left.x;
            maxX = right.x;
            minZ = down.z;
            maxZ = up.z;
        }
        else
        {
            Debug.LogError("MapOutLine がシーンに存在しません。");
        }

        StartCoroutine(SpawnMobsRoutine());
    }

    private IEnumerator SpawnMobsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnMob();
        }
    }

    private void SpawnMob()
    {
        if (mobPool.Count > 0)
        {
            const int maxAttempts = 10;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition(playerTransform.position, spawnRadius);

                if (!IsInsideBounds(spawnPos)) continue;

                // 判定用の仮の球の半径
                float checkRadius = 1f;

                // 重なり検出：周囲に当たる Collider を取得
                Collider[] colliders = Physics.OverlapSphere(spawnPos, checkRadius);
                bool isOverlapping = false;

                foreach (var col in colliders)
                {
                    if (col.CompareTag("Player") || col.CompareTag("Wall") || col.CompareTag("Roulette"))
                    {
                        isOverlapping = true;
                        break;
                    }
                }

                if (isOverlapping) continue;

                // 問題なければ生成
                GameObject mob = mobPool.Dequeue();
                mob.transform.position = spawnPos;
                mob.SetActive(true);
                activeMobs.Add(mob);
                return;
            }

            Debug.LogWarning("安全なスポーン位置が見つかりませんでした");
        }
    }


    private Vector3 GetRandomSpawnPosition(Vector3 center, float radius)
    {
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        return new Vector3(center.x + randomCircle.x, center.y, center.z + randomCircle.y);
    }

    private bool IsInsideBounds(Vector3 position)
    {
        return position.x >= minX && position.x <= maxX &&
               position.z >= minZ && position.z <= maxZ;
    }

    public void ReleaseMob(GameObject mob)
    {
        // スコア加算（Mobが倒されたとき）
        ScoreManager.Instance?.AddKill();
        //SE
        SoundSE.Instance?.Play("Explosion");

        mob.SetActive(false);
        activeMobs.Remove(mob);
        mobPool.Enqueue(mob);
    }
}
