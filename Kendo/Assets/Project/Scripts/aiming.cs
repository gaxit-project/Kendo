using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class aiming : MonoBehaviour
{
    [SerializeField] private InputActionReference _aimAction;
    [SerializeField] private float _aimSpeed = 500f;

    private Vector2 _aimInput;
    private RectTransform _rectTransform;
    private RectTransform _parentRectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentRectTransform = _rectTransform.parent.GetComponent<RectTransform>(); // 親（Canvasとか）も取る

        _aimAction.action.performed += OnMove;
        _aimAction.action.canceled += OnMove;
    }

    private void OnDestroy()
    {
        _aimAction.action.performed -= OnMove;
        _aimAction.action.canceled -= OnMove;
    }

    private void OnEnable() => _aimAction.action.Enable();
    private void OnDisable() => _aimAction.action.Disable();

    private void Update()
    {
        // 照準位置更新
        Vector2 moveDelta = _aimInput * _aimSpeed * Time.deltaTime;
        _rectTransform.anchoredPosition += moveDelta;

        // ここから画面端制御
        ClampPositionInsideParent();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _aimInput = context.ReadValue<Vector2>();
    }

    private void ClampPositionInsideParent()
    {
        Vector2 halfSize = _rectTransform.sizeDelta * 0.5f;
        Vector2 parentHalfSize = _parentRectTransform.rect.size * 0.5f;

        // 左右上下の最大移動範囲を計算
        float minX = -parentHalfSize.x + halfSize.x;
        float maxX = parentHalfSize.x - halfSize.x;
        float minY = -parentHalfSize.y + halfSize.y;
        float maxY = parentHalfSize.y - halfSize.y;

        Vector2 clampedPosition = _rectTransform.anchoredPosition;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);

        _rectTransform.anchoredPosition = clampedPosition;
    }
}
