using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    // InputActionAsset‚Ö‚ÌQÆ
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
        // x-z•½–Ê‚ÅˆÚ“®‚·‚é
        Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);

        // ˆÚ“®‚µ‚Ä‚¢‚é‚Æ‚«‚¾‚¯Œü‚«‚ğ•Ï‚¦‚é
        if (move.sqrMagnitude > 0.01f)
        {
            // ƒLƒƒƒ‰‚ÌŒü‚«‚ğˆÚ“®•ûŒü‚É•ÏX
            transform.forward = move.normalized;
        }

        // ÀÛ‚ÌˆÚ“®ˆ—
        transform.position += move * _moveSpeed * Time.deltaTime;
    }


    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }


}
