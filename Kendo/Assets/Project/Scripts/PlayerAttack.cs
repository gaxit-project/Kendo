using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private InputActionReference _attackAction;
    [SerializeField] private GameObject _target;
    [SerializeField] private float knockbackDistance = 10f;

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
        Debug.Log("Attack pressed!");

        Vector3 startPosition = transform.position + Vector3.up * 1f;
        Vector3 direction = transform.forward;

        Ray ray = new Ray(startPosition, direction);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 3f))
        {
            if (hitInfo.collider.CompareTag("Mob"))
            {
                Debug.Log("Mob hit!");

                test mobScript = hitInfo.collider.GetComponent<test>();
                if (mobScript != null)
                {
                    mobScript.Knockback(transform.forward); // プレイヤーの前方向を渡す
                }
            }
        }
    }


}
