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
        // �C�x���g�ɁA�R���[�`�����J�n���郁�\�b�h��o�^�i�w�ǁj
        OnReloadRequired += HandleReload;
    }

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    private void OnDestroy()
    {
        _attackAction.action.performed -= OnAttack;
        // �I�u�W�F�N�g�j�����ɃC�x���g�̓o�^������
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
    /// OnReloadRequired�C�x���g�ɉ������āA�����[�h�R���[�`�����J�n����
    /// </summary>
    private void HandleReload()
    {
        // �A���ŌĂ΂�Ă����v�Ȃ悤�ɁA�����[�h���łȂ���ΊJ�n����
        if (!_isReloading)
        {
            StartCoroutine(RelodeAmmo());
        }
    }

    /// <summary>
    /// �����[�h�������s���R���[�`��
    /// </summary>
    private IEnumerator RelodeAmmo()
    {
        _isReloading = true;
        Debug.Log("�����[�h�J�n...");
        SoundSE.Instance?.Play("reloadStart");

        // �w�肵�����Ԃ���������҂�
        yield return new WaitForSeconds(reloadTime);

        // �e���[
        currentAmmo = maxAmmo;
        _isReloading = false;

        Debug.Log("�����[�h�����I");
        SoundSE.Instance?.Play("reloadEnd");

    }
}