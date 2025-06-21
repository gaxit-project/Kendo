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
        _parentRectTransform = _rectTransform.parent.GetComponent<RectTransform>(); // �e�iCanvas�Ƃ��j�����

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
        // �Ə��ʒu�X�V
        Vector2 moveDelta = _aimInput * _aimSpeed * Time.deltaTime;
        _rectTransform.anchoredPosition += moveDelta;

        // ���������ʒ[����
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

        // ���E�㉺�̍ő�ړ��͈͂��v�Z
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
