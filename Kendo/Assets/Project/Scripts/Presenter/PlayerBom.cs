using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerBom : MonoBehaviour
{
    [SerializeField] private InputActionReference _bomAction;
    public static bool bom;

    [SerializeField] private int maxBomCount = 3;
    private int currentBomCount = 1;

    [SerializeField] private float bomDuration = 5f; // �{���̌��ʎ��ԁi�b�j

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
            //SE
            SoundSE.Instance?.Play("Bom");

            // ��莞�Ԍ�Ƀ{�����ʉ���
            StartCoroutine(BomCooldown());
        }
    }
    private IEnumerator BomCooldown()
    {
        yield return new WaitForSeconds(bomDuration);
        bom = false;
    }

    // �O������{����ǉ�����p
    public void AddBom()
    {
        if (currentBomCount < maxBomCount)
        {
            currentBomCount++;
        }
    }
    // �O���Q�Ɨp��Bom��
    public int GetBomCount()
    {
        return currentBomCount;
    }

    // UI���̕\�����t���b�V��
    public int GetMaxBomCount()
    {
        return maxBomCount;
    }

    //�K�`�����ʂŃ{�����o���ꍇ�ɑ��N������p
    public void GachaBom()
    {
        bom = true;
        BulletManager.Instance.ClearAllBullets();
        //SE
        SoundSE.Instance?.Play("Bom");

        // ��莞�Ԍ�Ƀ{�����ʉ���
        StartCoroutine(BomCooldown());
    }
}
