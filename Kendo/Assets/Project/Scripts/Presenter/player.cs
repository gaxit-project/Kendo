using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Main.Presenter;

public class player : MonoBehaviour
{
    public static player Instance { get; private set; }

    // InputActionAssetへの参照
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private float _moveSpeed = 5f;

    //テクスチャ変更用
    private Renderer rend;
    [SerializeField] private float switchInterval = 0.3f;
    private float timer = 0f;
    [SerializeField] private Texture idleTexture;
    [SerializeField] private Texture[] textures;
    private int textureIndex = 0;

    //無敵用
    [SerializeField] private float invincibleTime = 1.0f; // 無敵時間
    [SerializeField] private float blinkInterval = 0.1f;   // 点滅間隔
    private bool isInvincible = false; // 無敵フラグ
    
    private Vector2 _moveInput;

    //登場
    [SerializeField] private GameObject appearEffectPrefab; // 登場時エフェクト
    [SerializeField] private float appearDuration = 1.5f;    // 再生時間
    private bool _canMove = false; // 移動許可フラグ



    // --- マップ連携用 ---
    [Header("Map Dependencies")]
    [SerializeField] private MapPresenter mapPresenter;
    private float _currentMapHalfSize; // 現在のマップの半径（中心から端まで）
    private float _previousMapHalfSize; // 前フレームのマップの半径を保持
    
    [Header("Movement Smoothing")]

    [SerializeField] private float boundaryPushSmoothTime = 0.05f; 
    private Vector3 _playerPositionVelocity = Vector3.zero; // SmoothDampで使用

    private bool _isMapPresenterReady = false;

    private void Awake()
    {
        _moveAction.action.performed += OnMove;
        _moveAction.action.canceled += OnMoveCanceled;
        _moveAction.action.canceled += OnMove;
        rend = GetComponent<Renderer>();
        rend.material.mainTexture = idleTexture;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        // MapPresenterの参照を取得・設定
        if (mapPresenter == null)
        {
            mapPresenter = MapPresenter.Instance; // Singletonインスタンスを使用
        }

        if (mapPresenter != null)
        {
            if (mapPresenter.IsReady)
            {
                InitializeMapInteraction();
            }
            else
            {
                MapPresenter.OnMapPresenterReady += InitializeMapInteraction;
            }
        }
        else
        {
            Debug.LogError("[Player] MapPresenter is not assigned and not found. Movement will not be restricted to map boundaries.");
            // MapPresenterが見つからない場合、非常に大きな境界として扱うか、エラーとする
            _currentMapHalfSize = float.MaxValue; // 事実上、境界制限なし
            _isMapPresenterReady = true; // 処理を進めるためにReady扱いにするが、制限は効かない
        }
        //登場
        StartCoroutine(PlayAppearEffect());

    }

    private void OnDestroy()
    {
        _moveAction.action.performed -= OnMove;
        _moveAction.action.canceled -= OnMoveCanceled;
        _moveAction.action.canceled -= OnMove;
        
        if (mapPresenter != null)
        {
            mapPresenter.OnMapSizeUpdated -= OnMapSizeChangedByPresenter;
        }
        // MapPresenter.OnMapPresenterReady の購読解除 (Startで1回だけ呼ばれるハンドラの場合)
        // ただし、InitializeMapInteraction内で既に解除しているので、ここでは不要かもしれないが、念のため
        if (MapPresenter.Instance != null && !_isMapPresenterReady) // まだ初期化されていなければ
        {
            MapPresenter.OnMapPresenterReady -= InitializeMapInteraction;
        }
    }

    private void OnEnable() => _moveAction.action.Enable();
    private void OnDisable() => _moveAction.action.Disable();

    private void Update()
    {
        if (!_canMove) return;
        // x-z平面で移動する
        Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);
        Vector3 intendedMovementDelta = Vector3.zero; 

        // 移動しているときだけ向きを変える
        if (move.sqrMagnitude > 0.01f)
        {
            //移動中は炎が出てる2種類の画像を交互に切り替える
            timer += Time.deltaTime;
            if (timer >= switchInterval)
            {
                // テクスチャを交互に切り替える
                textureIndex = 1 - textureIndex;
                rend.material.mainTexture = textures[textureIndex];
                timer = 0f;
            }

            // キャラの向きを移動方向に変更
            transform.forward = move.normalized;
            intendedMovementDelta = move.normalized * (_moveSpeed * Time.deltaTime);
        }
        else
        {
            rend.material.mainTexture = idleTexture;
            timer = 0f;
            textureIndex = 0;
        }

        // 現在のマップ境界を計算
        float minX = -_currentMapHalfSize;
        float maxX = _currentMapHalfSize;
        float minZ = -_currentMapHalfSize;
        float maxZ = _currentMapHalfSize;

        // プレイヤーが壁なしで移動した場合の次の位置を計算
        Vector3 potentialNextPosition = transform.position + intendedMovementDelta;

        // マップがこのフレームで縮小したかどうかを判定
        //    _previousMapHalfSize が有効な値であることも確認
        bool mapShrankThisFrame = _currentMapHalfSize < _previousMapHalfSize && _previousMapHalfSize > 0.001f && _previousMapHalfSize != float.MaxValue;

        // プレイヤーの現在の位置が、新しい（縮小後の可能性のある）マップ境界の外にあるか判定
        bool isCurrentPositionOutOfBounds = 
            transform.position.x < minX || transform.position.x > maxX ||
            transform.position.z < minZ || transform.position.z > maxZ;

        // 移動ロジックの決定
        if (mapShrankThisFrame && isCurrentPositionOutOfBounds)
        {
            // --- マップが縮小し、かつプレイヤーが新しい境界の外側にいる場合：「ズルズル」と押し戻す ---
            Vector3 pushTargetPosition = new Vector3(
                Mathf.Clamp(transform.position.x, minX, maxX), // 現在の位置を新しい境界内にクランプした位置が目標
                transform.position.y,
                Mathf.Clamp(transform.position.z, minZ, maxZ)
            );

            transform.position = Vector3.SmoothDamp(
                transform.position,
                pushTargetPosition,
                ref _playerPositionVelocity,
                boundaryPushSmoothTime
            );
            // Debug.Log("Player being pushed by shrinking map.");
        }
        else
        {
            // --- 通常移動、マップ拡大時、またはマップ縮小したがプレイヤーは境界内の場合：直接移動し、境界でクランプ ---
            Vector3 finalPositionThisFrame = new Vector3(
                Mathf.Clamp(potentialNextPosition.x, minX, maxX), // 意図した移動先をクランプ
                transform.position.y,
                Mathf.Clamp(potentialNextPosition.z, minZ, maxZ)
            );
            transform.position = finalPositionThisFrame;
            _playerPositionVelocity = Vector3.zero; // SmoothDampを使用しないので速度をリセット
        }

        // 8. 次のフレームのために現在のマップサイズを「前回のサイズ」として保存
        _previousMapHalfSize = _currentMapHalfSize;
        
    }


    private void OnMove(InputAction.CallbackContext context)
    {
        if (Time.timeScale != 0)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
        
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _moveInput = Vector2.zero;
    }
    

    // 敵や敵弾との衝突でダメージ
    private void OnTriggerEnter(Collider other)
    {
        if (isInvincible) return; // 無敵中は返す

        if (other.CompareTag("Mob") || other.CompareTag("Bullet"))
        {
            StartCoroutine(InvincibleCoroutine());
            PlayerHP.Instance?.TakeDamage();

            if (other.CompareTag("Bullet"))
            {
                other.gameObject.SetActive(false);// 弾消す
            }
        }
    }

    //無敵処理
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibleTime)
        {
            rend.enabled = false; // レンダラーを非表示に
            yield return new WaitForSeconds(blinkInterval);
            rend.enabled = true; // レンダラーを表示
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval * 2;
        }

        rend.enabled = true;
        isInvincible = false;
    }
    
    private void InitializeMapInteraction()
    {
        if (_isMapPresenterReady) return; // 既に初期化済みなら何もしない

        if (mapPresenter == null && MapPresenter.Instance != null)
        {
            mapPresenter = MapPresenter.Instance;
        }
        
        if (mapPresenter == null) {
            Debug.LogError("[Player] MapPresenter became null before InitializeMapInteraction could complete. Movement restriction will fail.");
            _currentMapHalfSize = float.MaxValue;
            _isMapPresenterReady = true;
            return;
        }

        _currentMapHalfSize = mapPresenter.GetCurrentMapSize();
        _previousMapHalfSize = _currentMapHalfSize; 
        mapPresenter.OnMapSizeUpdated += OnMapSizeChangedByPresenter;
        _isMapPresenterReady = true;

        // イベントから一度だけ呼び出されるように、OnMapPresenterReadyの購読を解除
        MapPresenter.OnMapPresenterReady -= InitializeMapInteraction;
    }
    
    private void OnMapSizeChangedByPresenter(float newMapHalfSize)
    {
        _currentMapHalfSize = newMapHalfSize;
        // Debug.Log($"[Player] Map half size updated to: {newMapHalfSize}");
    }

    //スピード変更用
    public void ChangeSpeed()
    {
        _moveSpeed += 1f;
    }

    //登場
    private IEnumerator PlayAppearEffect()
    {
        SoundSE.Instance?.Play("warp");
        _canMove = false;
        SetPlayerAlpha(0f); // プレイヤー透明に

        GameObject effect = null;
        Renderer effectRenderer = null;

        if (appearEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 2.0f;
            Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);
            effect = Instantiate(appearEffectPrefab, spawnPosition, rotation);
            effectRenderer = effect.GetComponent<Renderer>();
        }

        // 同時にエフェクトを消しつつプレイヤーを出す
        float fadeDuration = appearDuration; // 両方同時に使う
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            // エフェクトを消す：1 → 0
            if (effectRenderer != null)
            {
                float effectAlpha = Mathf.Lerp(1f, 0f, t);
                SetAlpha(effectRenderer.material, effectAlpha);
            }

            // プレイヤーを出す：0 → 1
            float playerAlpha = Mathf.Lerp(0f, 1f, t);
            SetPlayerAlpha(playerAlpha);

            yield return null;
        }

        // 完了処理
        if (effect != null) Destroy(effect);
        SetPlayerAlpha(1f);
        _canMove = true;
    }

    // プレイヤーのマテリアル透明度を設定（0 = 完全透明, 1 = 完全表示）
    private void SetPlayerAlpha(float alpha)
    {
        if (rend != null && rend.material.HasProperty("_Color"))
        {
            Color c = rend.material.color;
            c.a = alpha;
            rend.material.color = c;
        }
    }

    // 任意のマテリアルの透明度を設定（エフェクト用など）
    private void SetAlpha(Material mat, float alpha)
    {
        if (mat != null && mat.HasProperty("_Color"))
        {
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;
        }
    }




}
