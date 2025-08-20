using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack Instance { get; private set; }

    [Header("Attack Settings")]
    [SerializeField] private InputActionReference _attackAction;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo;
    [SerializeField] private float reloadTime;
    
    private int currentAmmo;
    private bool _isReloading = false;
    
    // --- ↓UI通知用のイベントを追加↓ ---
    /// <summary>
    /// ゲーム開始時にUIを初期化するために発火 (最大弾数を渡す)
    /// </summary>
    public event Action<int> OnAmmoInitialized;
    
    /// <summary>
    /// 弾を発射した時に発火 (残りの弾数を渡す)
    /// </summary>
    public event Action<int> OnShotFired;
    
    /// <summary>
    /// リロードが1発分進むたびに発火 (リロードされた弾のインデックスを渡す)
    /// </summary>
    public event Action<int> OnReloadProgress;
    
    /// <summary>
    /// リロードが完了した時に発火
    /// </summary>
    public event Action OnReloadComplete;
    // --- ↑UI通知用のイベントを追加↑ ---

    public int GetCurrentAmmo() => currentAmmo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _attackAction.action.performed += OnAttack;
    }

    private void Start()
    {
        currentAmmo = maxAmmo;
        // UIの初期化イベントを発火
        OnAmmoInitialized?.Invoke(maxAmmo);
    }

    private void OnDestroy()
    {
        _attackAction.action.performed -= OnAttack;
    }

    private void OnEnable() => _attackAction.action.Enable();
    private void OnDisable() => _attackAction.action.Disable();

    private void OnAttack(InputAction.CallbackContext context)
    {
        // リロード中は何もしない
        if (_isReloading)
        {
            SoundSE.Instance?.Play("NoAmmo");
            return;
        }
        
        if (Time.timeScale != 0)
        {
            if (projectilePrefab == null || firePoint == null)
            {
                Debug.LogWarning("ProjectilePrefab or FirePoint not assigned");
                return;
            }

            if (currentAmmo > 0)
            {
                GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                projectile.GetComponent<Projectile>().Initialize(transform.forward);
                SoundSE.Instance?.Play("Shot");

                if (GachaManager.Instance.isInvincible)
                {
                    // 777中は弾は無限
                }
                else
                {
                    currentAmmo--;
                }
                
                
                // UIへ発射を通知
                OnShotFired?.Invoke(currentAmmo);

                if (currentAmmo <= 0)
                {
                    HandleReload();
                }
            }
            else
            {
                // 弾が0の時に撃とうとしたらリロード開始
                HandleReload();
                SoundSE.Instance?.Play("NoAmmo");
            }
        }
    }
    
    private void HandleReload()
    {
        if (!_isReloading)
        {
            StartCoroutine(ReloadAmmoCoroutine());
        }
    }

    /// <summary>
    /// リロード処理を行うコルーチン (1発ずつ処理するように変更)
    /// </summary>
    private IEnumerator ReloadAmmoCoroutine()
    {
        _isReloading = true;
        Debug.Log("リロード開始...");
        SoundSE.Instance?.Play("reloadStart");

        // 1発あたりのリロード時間を計算
        float timePerBullet = reloadTime / maxAmmo;

        // 1発ずつ弾を込めるループ
        for (int i = 0; i < maxAmmo; i++)
        {
            yield return new WaitForSeconds(timePerBullet);
            
            // UIへリロードの進捗を通知
            OnReloadProgress?.Invoke(i);
        }

        // 弾を完全に補充
        currentAmmo = maxAmmo;
        _isReloading = false;

        // UIへリロード完了を通知
        OnReloadComplete?.Invoke();
        
        Debug.Log("リロード完了！");
        SoundSE.Instance?.Play("reloadEnd");
    }
}