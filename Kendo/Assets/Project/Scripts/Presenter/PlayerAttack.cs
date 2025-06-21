using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private InputActionReference _attackAction;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private void Awake()
    {
        _attackAction.action.performed += OnAttack;
    }

    private void OnDestroy()
    {
        _attackAction.action.performed -= OnAttack;
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

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            projectile.GetComponent<Projectile>().Initialize(transform.forward);
            //SE
            SoundSE.Instance?.Play("Shot");
        }
    }
}
