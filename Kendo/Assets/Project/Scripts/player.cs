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
        Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);

        // �ړ����Ă���Ƃ�����������ς���
        if (move.sqrMagnitude > 0.01f)
        {
            // �L�����̌������ړ������ɕύX
            transform.forward = move.normalized;
        }

        // ���ۂ̈ړ�����
        transform.position += move * _moveSpeed * Time.deltaTime;
    }


    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }


}
