using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBom : MonoBehaviour
{
    [SerializeField] private InputActionReference _bomAction;
    public static bool bom;

    [SerializeField] private int maxBomCount = 3;
    private int currentBomCount = 1;

    public static PlayerBom Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

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
        if (!bom && currentBomCount > 0)
        {
            bom = true;
            currentBomCount--;
            BulletManager.Instance.ClearAllBullets();
        }
    }
    // 外部からボムを追加する用
    public void AddBom()
    {
        if (currentBomCount < maxBomCount)
        {
            currentBomCount++;
        }
    }
    // 外部参照用のBom個数
    public int GetBomCount()
    {
        return currentBomCount;
    }

    // UI等の表示リフレッシュ
    public int GetMaxBomCount()
    {
        return maxBomCount;
    }
}
