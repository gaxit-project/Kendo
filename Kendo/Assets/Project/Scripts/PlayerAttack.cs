using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private InputActionReference _attackAction;
    [SerializeField] private GameObject _target;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _attackAction.action.performed += OnAttack;
        _rectTransform = _target.GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        _attackAction.action.performed -= OnAttack;
    }

    private void OnEnable() => _attackAction.action.Enable();
    private void OnDisable() => _attackAction.action.Disable();

    private void OnAttack(InputAction.CallbackContext context)
    {
        // ボタンが押されたときだけ呼ばれる
        Debug.Log("Attack pressed!");

        // 射出するスタート地点
        Vector3 startPosition = new Vector3(_rectTransform.anchoredPosition.x,25f,_rectTransform.anchoredPosition.y);

        // 下向きに30fのRayを撃つ
        Ray ray = new Ray(startPosition, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 30f))
        {
            if (hitInfo.collider.CompareTag("Mob"))
            {
                Debug.Log("Mob hit!");
                hitInfo.collider.gameObject.SetActive(false);
            }
        }
    }
}
