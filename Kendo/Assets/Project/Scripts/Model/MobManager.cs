using System.Collections;
using System.Collections.Generic;
using Main.Presenter;
// MapPresenterがMain.Model名前空間にあるため、usingディレクティブは不要な場合もありますが、
// 明示的に追加するか、MapPresenterのnamespaceに合わせて調整してください。
// using Main.Model; // MapPresenterがこの名前空間にある場合
using UnityEngine;

public class MobManager : MonoBehaviour
{
    public static MobManager Instance { get; private set; }

    [SerializeField] private GameObject mobPrefab;
    [SerializeField] private int poolSize = 100;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnRadius = 20f; // プレイヤーを中心としたMobの出現半径
    [SerializeField] private Transform playerTransform;

    [SerializeField] private GameObject destroyEffectPrefab;
    
    [SerializeField] private MapPresenter mapPresenter;

    private Queue<GameObject> mobPool = new Queue<GameObject>();
    private List<GameObject> activeMobs = new List<GameObject>();

    // マップの境界
    private float minX, maxX, minZ, maxZ;

    // MapPresenterが見つからなかった場合のフォールバック用の境界サイズ
    [Header("Fallback Map Settings")]
    [SerializeField] private float defaultMapBoundarySize = 40f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeMobPool();
    }
    
    private void Start()
    {
        if (mapPresenter == null)
        {
            mapPresenter = MapPresenter.Instance;
        }

        if (mapPresenter != null)
        {
            if (mapPresenter.IsReady)
            {
                SetupMapBoundaries();
            }
            else
            {
                MapPresenter.OnMapPresenterReady += SetupMapBoundaries;
            }
        }
        StartCoroutine(SpawnMobsRoutine());
    }

    private void InitializeMobPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject mob = Instantiate(mobPrefab);
            mob.SetActive(false);
            mobPool.Enqueue(mob);
        }
    }

    /// <summary>
    /// マップの境界を設定します。MapPresenterから取得し、失敗した場合はデフォルト値を使用します。
    /// </summary>
    private void SetupMapBoundaries()
    {
        if (mapPresenter == null && MapPresenter.Instance != null)
        {
            mapPresenter = MapPresenter.Instance;
        }
        
        try
        {
            float currentMapHalfSize = mapPresenter.GetCurrentMapSize();
            minX = -currentMapHalfSize;
            maxX = currentMapHalfSize;
            minZ = -currentMapHalfSize;
            maxZ = currentMapHalfSize;
        }
        catch (System.NullReferenceException ex)
        {
            minX = -defaultMapBoundarySize;
            maxX = defaultMapBoundarySize;
            minZ = -defaultMapBoundarySize;
            maxZ = defaultMapBoundarySize;
        }
        // イベントから一度だけ呼び出されるように、購読を解除する
        MapPresenter.OnMapPresenterReady -= SetupMapBoundaries;
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
            const int maxAttempts = 10; // スポーン位置探査の最大試行回数

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // プレイヤーの位置から一定のspawnRadius内にランダムな位置を生成
                Vector3 spawnPos = GetRandomSpawnPositionAroundPlayer(playerTransform.position, spawnRadius);

                // 生成位置がマップ境界内にあるか確認
                if (!IsInsideMapBounds(spawnPos))
                {
                    continue;
                }

                // 他の重要オブジェクトと重なっていないか確認
                if (IsOverlappingWithCriticalObjects(spawnPos))
                {
                    continue;
                }

                // 問題なければMobを配置
                GameObject mob = mobPool.Dequeue();
                mob.transform.position = spawnPos;
                mob.SetActive(true);
                activeMobs.Add(mob);
                return; // Mobをスポーンしたらループを抜ける
            }
        }
    }

    /// <summary>
    /// プレイヤーの周囲の指定された半径内にランダムなスポーン位置を取得します。
    /// </summary>
    private Vector3 GetRandomSpawnPositionAroundPlayer(Vector3 playerPos, float radius)
    {
        Vector2 randomCirclePoint = Random.insideUnitCircle * radius;
        // Y座標はプレイヤーと同じか、地面に合わせるなど調整が必要な場合は変更
        return new Vector3(playerPos.x + randomCirclePoint.x, playerPos.y, playerPos.z + randomCirclePoint.y);
    }

    /// <summary>
    /// 指定された位置がマップ境界内にあるかどうかを判定します。
    /// </summary>
    private bool IsInsideMapBounds(Vector3 position)
    {
        return position.x >= minX && position.x <= maxX &&
               position.z >= minZ && position.z <= maxZ;
    }

    /// <summary>
    /// 指定された位置が他の重要オブジェクトと重なっているかどうかを判定します。
    /// </summary>
    private bool IsOverlappingWithCriticalObjects(Vector3 position)
    {
        // 判定用の仮の球の半径（Mobのサイズに合わせて調整）
        float checkRadius = 1f; 
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius);
        foreach (var col in colliders)
        {
            // プレイヤー、壁、または他のMobなど、スポーン時に重なりを避けたいタグを指定
            if (col.CompareTag("Player") || col.CompareTag("Wall") || col.CompareTag("Roulette") || col.CompareTag("Mob")) // "Mob"タグも追加検討
            {
                return true; // 重なっている
            }
        }
        return false; // 重なっていない
    }

    public void ReleaseMob(GameObject mob)
    {
        ScoreManager.Instance?.AddKill();
        SoundSE.Instance?.Play("Explosion");

        if (destroyEffectPrefab != null)
        {
            Vector3 effectPos = mob.transform.position;
            Quaternion effectRot = Quaternion.Euler(90f, 0f, 0f);
            GameObject effect = Instantiate(destroyEffectPrefab, effectPos, effectRot);
            effect.transform.localScale *= 2f;
            Destroy(effect, 1f);
        }

        mob.SetActive(false);
        if(activeMobs.Contains(mob)) // リストに存在する場合のみ削除
        {
            activeMobs.Remove(mob);
        }
        mobPool.Enqueue(mob);
    }
    //スコアに加算しない死
    public void ReleaseMobWithoutScore(GameObject mob)
    {
        SoundSE.Instance?.Play("Explosion");

        if (destroyEffectPrefab != null)
        {
            Vector3 effectPos = mob.transform.position;
            Quaternion effectRot = Quaternion.Euler(90f, 0f, 0f);
            GameObject effect = Instantiate(destroyEffectPrefab, effectPos, effectRot);
            effect.transform.localScale *= 2f;
            Destroy(effect, 1f);
        }

        mob.SetActive(false);
        if (activeMobs.Contains(mob))
        {
            activeMobs.Remove(mob);
        }
        mobPool.Enqueue(mob);
    }

}