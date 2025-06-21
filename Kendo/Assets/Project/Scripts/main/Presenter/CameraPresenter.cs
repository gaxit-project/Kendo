using UnityEngine;
using Main.Presenter;

/// <summary>
/// CameraViewの初期化と更新呼び出しを管理します。
/// FadeableUIManagerを介してUIのフェードを制御します。
/// </summary>
public class CameraPresenter : MonoBehaviour
{
    private GameObject _player;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 30, 0);

    [Header("Dependencies")]
    [SerializeField] private CameraView view;
    [SerializeField] private MapPresenter mapPresenter;
    [SerializeField] private Camera cameraToControl;

    // FadeableUIのリストはマネージャーが持っているため、このスクリプト内での保持は不要

    void Awake()
    {
        if (view == null) { /* ... */ return; }
        if (mapPresenter == null) { mapPresenter = FindObjectOfType<MapPresenter>(); if (mapPresenter == null) { /* ... */ return; } }
        if (cameraToControl == null) { cameraToControl = GetComponent<Camera>(); if (cameraToControl == null) cameraToControl = Camera.main; if (cameraToControl == null) { /* ... */ return; } }
    }

    void Start()
    {
        _player = GameObject.FindWithTag("Player");
        if (_player == null) { /* ... */ return; }

        view.Initialize(_player, offset, mapPresenter, cameraToControl);

        // FindObjectsOfTypeの呼び出しは不要
    }

    void LateUpdate()
    {
        if (view != null && view.enabled)
        {
            view.SetCameraPosition();
        }

        // UIのフェード処理
        HandleUIFadingForOverlay();
    }

    /// <summary>
    /// 【Overlay用】プレイヤーのスクリーン座標とUIの矩形を比較し、フェード処理を管理します。
    /// </summary>
    private void HandleUIFadingForOverlay()
    {
        if (_player == null || cameraToControl == null) return;

        Vector2 playerScreenPos = cameraToControl.WorldToScreenPoint(_player.transform.position);

        // FindObjectsOfTypeの代わりに、マネージャーのリストを直接参照する
        foreach (var fadeable in FadeableUIManager.Instances)
        {
            if (fadeable == null || !fadeable.gameObject.activeInHierarchy) continue;

            RectTransform uiRect = fadeable.GetComponent<RectTransform>();

            if (RectTransformUtility.RectangleContainsScreenPoint(uiRect, playerScreenPos, null))
            {
                fadeable.FadeOut();
            }
            else
            {
                fadeable.FadeIn();
            }
        }
    }
}