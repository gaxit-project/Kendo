using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    // InputActionAsset�ւ̎Q��
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private float _moveSpeed = 5f;

    private Vector2 _moveInput;

    private void Awake()
    {
        _moveAction.action.performed += OnMove;
        _moveAction.action.canceled += OnMove;
    }

    private void OnDestroy()
    {
        _moveAction.action.performed -= OnMove;
        _moveAction.action.canceled -= OnMove;
    }

    private void OnEnable() => _moveAction.action.Enable();
    private void OnDisable() => _moveAction.action.Disable();

    private void Update()
    {
        // x-z���ʂňړ�����
        Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y) * _moveSpeed * Time.deltaTime;
        transform.position += move;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }
}
