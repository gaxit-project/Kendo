using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack Instance { get; private set; }

    [SerializeField] private InputActionReference _attackAction;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int maxAmmo;
    [SerializeField] private int currentAmmo;
    [SerializeField] private float reloadTime;

    public event Action OnReloadRequired;
    private bool _isReloading = false;

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;


        _attackAction.action.performed += OnAttack;
        // イベントに、コルーチンを開始するメソッドを登録（購読）
        OnReloadRequired += HandleReload;
    }

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    private void OnDestroy()
    {
        _attackAction.action.performed -= OnAttack;
        // オブジェクト破棄時にイベントの登録を解除
        OnReloadRequired -= HandleReload;
    }

    private void OnEnable() => _attackAction.action.Enable();
    private void OnDisable() => _attackAction.action.Disable();

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (Time.timeScale != 0)
        {
            if (projectilePrefab == null || firePoint == null)
            {
                Debug.LogWarning("ProjectilePrefab or FirePoint not assigned");
                return;
            }


            if(0 < currentAmmo)
            {
                // 

                GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                projectile.GetComponent<Projectile>().Initialize(transform.forward);
                //SE
                SoundSE.Instance?.Play("Shot");

                currentAmmo--;

                if (currentAmmo >= 0)
                {
                    OnReloadRequired?.Invoke();
                }
            }
            else
            {
                SoundSE.Instance?.Play("NoAmmo");
            }
        }
    }

    /// <summary>
    /// OnReloadRequiredイベントに応答して、リロードコルーチンを開始する
    /// </summary>
    private void HandleReload()
    {
        // 連続で呼ばれても大丈夫なように、リロード中でなければ開始する
        if (!_isReloading)
        {
            StartCoroutine(RelodeAmmo());
        }
    }

    /// <summary>
    /// リロード処理を行うコルーチン
    /// </summary>
    private IEnumerator RelodeAmmo()
    {
        _isReloading = true;
        Debug.Log("リロード開始...");
        SoundSE.Instance?.Play("reloadStart");

        // 指定した時間だけ処理を待つ
        yield return new WaitForSeconds(reloadTime);

        // 弾を補充
        currentAmmo = maxAmmo;
        _isReloading = false;

        Debug.Log("リロード完了！");
        SoundSE.Instance?.Play("reloadEnd");

    }
}