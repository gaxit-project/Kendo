using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBom : MonoBehaviour
{
    [SerializeField] private InputActionReference _bomAction;
    public static bool bom;

    private void Awake()
    {
        _bomAction.action.performed += OnBom;
        bom = false;
    }

    private void OnDestroy()
    {
        _bomAction.action.performed -= OnBom;
    }

    private void OnEnable() => _bomAction.action.Enable();
    private void OnDisable() => _bomAction.action.Disable();

    private void OnBom(InputAction.CallbackContext context)
    {
        if (!bom)
        {
            bom = true;
            BulletManager.Instance.ClearAllBullets();
        }
    }
}
