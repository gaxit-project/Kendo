using UnityEngine;
using UnityEngine.UI;

public class AmmoView : MonoBehaviour
{
    [SerializeField] private Image[] AmmoIcons;
    
    // 色を定数として定義
    private readonly Color _normalColor = new Color(127 / 255f, 255 / 255f, 105 / 255f);
    private readonly Color _reloadingColor = Color.white;

    private void Start()
    {
        // PlayerAttackのイベントに関数を登録（購読）
        if (PlayerAttack.Instance != null)
        {
            PlayerAttack.Instance.OnAmmoInitialized += InitializeAmmo;
            PlayerAttack.Instance.OnShotFired += UpdateAmmoDisplay;
            PlayerAttack.Instance.OnReloadProgress += ReloadOneBullet;
            PlayerAttack.Instance.OnReloadComplete += FinalizeReload;
        }
    }
    
    private void OnDestroy()
    {
        // オブジェクト破棄時にイベントの登録を解除
        if (PlayerAttack.Instance != null)
        {
            PlayerAttack.Instance.OnAmmoInitialized -= InitializeAmmo;
            PlayerAttack.Instance.OnShotFired -= UpdateAmmoDisplay;
            PlayerAttack.Instance.OnReloadProgress -= ReloadOneBullet;
            PlayerAttack.Instance.OnReloadComplete -= FinalizeReload;
        }
    }

    /// <summary>
    /// UIを初期状態にする
    /// </summary>
    private void InitializeAmmo(int maxAmmo)
    {
        for (int i = 0; i < AmmoIcons.Length; i++)
        {
            bool shouldBeVisible = i < maxAmmo;
            AmmoIcons[i].enabled = shouldBeVisible;
            if (shouldBeVisible)
            {
                AmmoIcons[i].color = _normalColor;
            }
        }
    }

    /// <summary>
    /// 弾が撃たれた時の処理
    /// </summary>
    private void UpdateAmmoDisplay(int currentAmmo)
    {
        // currentAmmoのインデックスにあるアイコンを非表示にする
        if (currentAmmo < AmmoIcons.Length)
        {
            AmmoIcons[currentAmmo].enabled = false;
        }
    }
    
    /// <summary>
    /// リロード中に弾を1発ずつ再描画する
    /// </summary>
    private void ReloadOneBullet(int reloadedIndex)
    {
        // 逆順に表示するためのインデックス計算
        int iconIndex = reloadedIndex;

        if (iconIndex >= 0 && iconIndex < AmmoIcons.Length)
        {
            AmmoIcons[iconIndex].enabled = true;
            AmmoIcons[iconIndex].color = _reloadingColor;
        }
    }

    /// <summary>
    /// リロードが完了した時の処理
    /// </summary>
    private void FinalizeReload()
    {
        // 全ての弾を緑色で表示する
        foreach (var icon in AmmoIcons)
        {
            icon.enabled = true;
            icon.color = _normalColor;
        }
    }
}