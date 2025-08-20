using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InGame.Model;
using Main.Presenter;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class MobManager : MonoBehaviour
{
    public static MobManager Instance { get; private set; }
    
    [Header("遠距離Mob設定")]
    [SerializeField] private GameObject rangedMobPrefab;
    [SerializeField] private int rangedPoolSize = 100;
    [SerializeField] private float rangedSpawnInterval = 5f;

    [Header("突進Mob設定")]
    [SerializeField] private GameObject tackleMobPrefab;
    [SerializeField] private int tacklePoolSize = 50;
    [SerializeField] private float tackleSpawnInterval = 8f;
    
    [Header("Mobの形態時間(秒)")]
    [SerializeField] private float mobStateInterval1 = 60f;
    [SerializeField] private float mobStateInterval2 = 180f;
    [SerializeField] private float mobStateInterval3 = 300f;
    [SerializeField] private float mobStateInterval4 = 480f;
    
    
    [SerializeField] private Transform playerTransform; // Y座標の基準として使用

    [SerializeField] private GameObject destroyEffectPrefab;
    
    [SerializeField] private MapPresenter mapPresenter;
    private Camera mainCamera; // スポーン判定に使用するカメラ

    
    private Queue<GameObject> _rangedMobPool = new Queue<GameObject>();
    private Queue<GameObject> _tackleMobPool = new Queue<GameObject>();
    private List<GameObject> activeMobs = new List<GameObject>();
    
    private float timer = 0f; // この変数はUpdateロジック移行に伴い、現在は未使用

    // 各Mobの攻撃タイマーを管理するDictionary
    private Dictionary<GameObject, float> _attackTimers = new Dictionary<GameObject, float>();
    
    private Dictionary<IEnemyModel, MobController> _modelToControllerMap = new Dictionary<IEnemyModel, MobController>();

    #region アクセスメソッド

    public float GetMobStateInterval1() => mobStateInterval1;
    public float GetMobStateInterval2() => mobStateInterval2;
    public float GetMobStateInterval3() => mobStateInterval3;
    public float GetMobStateInterval4() => mobStateInterval4;
    
    #endregion

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
        // カメラが設定されていなければメインカメラを自動で取得
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

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
        SpawnRangedMobsLoop().Forget();
        SpawnTackleMobsLoop().Forget();
    }

    private void Update()
    {
        float gameTimer = ScoreManager.Instance.GetMinutes() * 60 + ScoreManager.Instance.GetSeconds();
        if (gameTimer > mobStateInterval2)
        {
            rangedSpawnInterval = 3f;
            tackleSpawnInterval = 3f;
        }
        else if (gameTimer > mobStateInterval1)
        {
            rangedSpawnInterval = 4f;
            tackleSpawnInterval = 4f;
        }
        
        // 稼働中のMobリストを逆順にループ（途中でリストから削除されても安全なように）
        for (int i = activeMobs.Count - 1; i >= 0; i--)
        {
            GameObject mobGo = activeMobs[i];
            if (mobGo == null || !mobGo.activeInHierarchy) continue;
            
            MobController mobController = mobGo.GetComponent<MobController>();
            PhysicsModel physicsModel = mobController.GetPhysicsModel();
            IEnemyModel enemyModel = mobController.GetEnemyModel();
            
            if (physicsModel.GetIsKnockback())
            {
                // --- ノックバック中のロジック ---
                if (physicsModel.GetCurrentVelocity().magnitude < enemyModel.GetStopThreshold())
                {
                    mobController.StopKnockback();
                }
                else
                {
                    // 速度と抵抗に基づいて位置を更新
                    mobGo.transform.position += physicsModel.GetCurrentVelocity() * Time.deltaTime;
                    physicsModel.UpdateVelocityWithDrag(enemyModel.GetDrag());

                    // 壁との反射チェック
                    if (mobController.IsHitWall(out Vector3 wallNormal))
                    {
                        mobGo.transform.position += wallNormal * 0.05f; // 壁へのめり込み防止
                        physicsModel.SetCurrentVelocity(Vector3.Reflect(physicsModel.GetCurrentVelocity(), wallNormal) * enemyModel.GetRestitution());
                        physicsModel.IncrementBounceCount();
                        SoundSE.Instance?.Play("Elect");

                        if (physicsModel.GetBounceCount() >= enemyModel.GetMaxBounceCount())
                        {
                            ReleaseMob(mobGo);
                        }
                    }
                }
            }
            else
            {
                // --- 通常時の行動 ---
                enemyModel.UpdateAttackState(gameTimer);

                if (playerTransform != null && !PlayerBom.bom)
                {
                    // プレイヤーの方向を向く
                    Vector3 lookPos = playerTransform.position;
                    lookPos.y = mobGo.transform.position.y;
                    if (mobGo.GetComponent<MobController>().GetEnemyType() == MobController.EnemyType.Ranged)
                    {
                        mobGo.transform.LookAt(lookPos);
                    }
                    
                    
                    // 攻撃タイマーを更新
                    float currentAttackTime = _attackTimers[mobGo];
                    currentAttackTime += Time.deltaTime;
                    _attackTimers[mobGo] = currentAttackTime;

                    // 攻撃間隔に達したら攻撃実行
                    if (currentAttackTime >= enemyModel.GetAttackSpan())
                    {
                        enemyModel.ExecuteAttack(mobController, mobGo.GetCancellationTokenOnDestroy()).Forget();
                        _attackTimers[mobGo] = 0f; // タイマーリセット
                    }
                }
            }
        }
    }

    /// <summary>
    /// オブジェクトプールの初期化
    /// </summary>
    private void InitializeMobPool()
    {
        // 遠距離Mobのプール
        for (int i = 0; i < rangedPoolSize; i++)
        {
            GameObject mob = Instantiate(rangedMobPrefab);
            mob.SetActive(false);
            _rangedMobPool.Enqueue(mob);
            _attackTimers.Add(mob, 0f);
        }
        // 突進Mobのプール
        for (int i = 0; i < tacklePoolSize; i++)
        {
            GameObject mob = Instantiate(tackleMobPrefab);
            mob.SetActive(false);
            _tackleMobPool.Enqueue(mob);
            _attackTimers.Add(mob, 0f); // 攻撃はしないが、Dictionaryのキーとして登録は必要
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
        catch (System.NullReferenceException)
        {
            Debug.LogWarning("MapPresenterからマップサイズの取得に失敗。Fallback値を使用します。");
            minX = -defaultMapBoundarySize;
            maxX = defaultMapBoundarySize;
            minZ = -defaultMapBoundarySize;
            maxZ = defaultMapBoundarySize;
        }
        // イベントから一度だけ呼び出されるように、購読を解除する
        MapPresenter.OnMapPresenterReady -= SetupMapBoundaries;
    }

    
    /// <summary>
    /// 遠距離Mobを一定間隔で生成し続ける非同期ループ
    /// </summary>
    private async UniTask SpawnRangedMobsLoop()
    {
        // このオブジェクトが破棄されたときにループを停止するためのキャンセルトークンを取得
        var ct = this.GetCancellationTokenOnDestroy();

        while (true) // 無限ループ
        {
            // 指定した秒数だけ待機する。ctを渡すことで、待機中にオブジェクトが破棄されても安全に停止する
            await UniTask.Delay(TimeSpan.FromSeconds(rangedSpawnInterval), cancellationToken: ct);
            
            // ループがキャンセルされていなければMobを生成
            if (ct.IsCancellationRequested) break;
            
            SpawnMob(_rangedMobPool, rangedMobPrefab);
        }
    }

    /// <summary>
    /// 突進Mobを一定間隔で生成し続ける非同期ループ
    /// </summary>
    private async UniTask SpawnTackleMobsLoop()
    {
        var ct = this.GetCancellationTokenOnDestroy();

        while (true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(tackleSpawnInterval), cancellationToken: ct);
            
            if (ct.IsCancellationRequested) break;
            
            SpawnMob(_tackleMobPool, tackleMobPrefab);
        }
    }
    
    
    /// <summary>
    /// モブを「マップ内」「カメラ外」「重なり無し」の条件でスポーン
    /// </summary>
    private void SpawnMob(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0 && mainCamera != null)
        {
            const int maxAttempts = 10; // スポーン位置探査の最大試行回数

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // マップ境界内でランダムな位置を生成
                float randomX = Random.Range(minX, maxX);
                float randomZ = Random.Range(minZ, maxZ);
                Vector3 spawnPos = new Vector3(randomX, playerTransform.position.y, randomZ);

                // カメラの視野外か判定
                Vector3 viewportPoint = mainCamera.WorldToViewportPoint(spawnPos);
                // ビューポート座標のXとYが0-1の範囲内にある場合、カメラの視野内と判定
                // (viewportPoint.z > 0 はカメラの前方にあることを確認する条件)
                bool isInCameraView = viewportPoint.z > 0 && 
                                      viewportPoint.x >= 0 && viewportPoint.x <= 1 && 
                                      viewportPoint.y >= 0 && viewportPoint.y <= 1;

                if (isInCameraView)
                {
                    continue; // カメラ内なので再試行
                }

                // 他の重要オブジェクトと重なっていないか確認
                if (IsOverlappingWithCriticalObjects(spawnPos))
                {
                    continue;
                }

                // すべての条件をクリアしたらMobを配置
                GameObject mob = pool.Dequeue();
                
                var mobController = mob.GetComponent<MobController>();
                var enemyModel = mobController.GetEnemyModel();
        
                // マップに追加
                _modelToControllerMap.Add(enemyModel, mobController);
                
                mob.transform.position = spawnPos;
                mob.transform.localScale = prefab.transform.localScale;
                
                mob.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                
                mob.SetActive(true);
                activeMobs.Add(mob);
                _attackTimers[mob] = 0f; // スポーン時に攻撃タイマーをリセット
                return;
            }
        }
    }

    /// <summary>
    /// 指定された位置がマップ境界内にあるかどうかを判定します。（このメソッドは現在SpawnMob内では使用されませんが、他の用途のために残しています）
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
            // スポーン時に重なりを避けたいタグを指定
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
        
        
        MobController controller = mob.GetComponent<MobController>();
        if (controller != null)
        {
            
            var enemyModel = controller.GetEnemyModel();
            if (enemyModel != null && _modelToControllerMap.ContainsKey(enemyModel))
            {
                _modelToControllerMap.Remove(enemyModel);
            }
            
            if (controller.GetEnemyType() == MobController.EnemyType.Ranged)
            {
                _rangedMobPool.Enqueue(mob);
            }
            else // (controller.type == MobController.EnemyType.Tackle)
            {
                _tackleMobPool.Enqueue(mob);
            }
        }
        else
        {
            // フォールバック（万が一MobControllerが取得できない場合）
            // プレハブ名などで判定も可能ですが、ここでは片方のプールに仮で戻します
            Debug.LogWarning($"MobControllerが見つかりませんでした: {mob.name}");
            _rangedMobPool.Enqueue(mob); 
        }
    }
    
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
        
        MobController controller = mob.GetComponent<MobController>();
        if (controller != null)
        {
            var enemyModel = controller.GetEnemyModel();
            if (enemyModel != null && _modelToControllerMap.ContainsKey(enemyModel))
            {
                _modelToControllerMap.Remove(enemyModel);
            } 
            
            if (controller.GetEnemyType() == MobController.EnemyType.Ranged)
            {
                _rangedMobPool.Enqueue(mob);
            }
            else // (controller.type == MobController.EnemyType.Tackle)
            {
                _tackleMobPool.Enqueue(mob);
            }
        }
        else
        {
            // フォールバック（万が一MobControllerが取得できない場合）
            // プレハブ名などで判定も可能ですが、ここでは片方のプールに仮で戻します
            Debug.LogWarning($"MobControllerが見つかりませんでした: {mob.name}");
            _rangedMobPool.Enqueue(mob); 
        }
    }
    
    /// <summary>
    /// モデルから対応するMobControllerインスタンスを取得する
    /// </summary>
    public MobController GetActiveMobControllerByModel(IEnemyModel model)
    {
        _modelToControllerMap.TryGetValue(model, out var controller);
        return controller;
    }
    
    /// <summary>
    /// プレイヤーのTransformを取得する
    /// </summary>
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }
}