using UnityEngine;
using Main.Presenter;

/// <summary>
/// CameraViewの初期化と更新呼び出しを管理します。
/// Screen Space - Overlay のUIがプレイヤーを遮る際に半透明にする機能も担当します。
/// </summary>
public class CameraPresenter : MonoBehaviour
{
    private GameObject _player; // 追従対象のプレイヤー

    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 30, 0); 

    [Header("Dependencies")]
    [SerializeField] private CameraView view; 
    [SerializeField] private MapPresenter mapPresenter; 
    [SerializeField] private Camera cameraToControl; 
    
    // --- ★UIフェード関連 ---
    // シーンに存在する全てのFadeableUIコンポーネントをキャッシュするための配列
    private FadeableUI[] _allFadeableUis;

    void Awake()
    {
        // (既存のAwakeのコードは変更なし)
        if (view == null)
        {
            Debug.LogError("[CameraPresenter] CameraView component is not assigned in the Inspector.");
            enabled = false; 
            return;
        }

        if (mapPresenter == null)
        {
            mapPresenter = FindObjectOfType<MapPresenter>();
            if (mapPresenter == null)
            {
                Debug.LogError("[CameraPresenter] MapPresenter is not assigned and could not be found in the scene.");
                enabled = false;
                return;
            }
        }

        if (cameraToControl == null)
        {
            cameraToControl = GetComponent<Camera>();
            if (cameraToControl == null) cameraToControl = Camera.main;

            if (cameraToControl == null)
            {
                Debug.LogError("[CameraPresenter] CameraToControl is not assigned and no Camera component found on this GameObject or as Camera.main.");
                enabled = false;
                return;
            }
        }
    }

    void Start()
    {
        // プレイヤーをタグで検索
        _player = GameObject.FindWithTag("Player"); 
        if (_player == null)
        {
            Debug.LogError("[CameraPresenter] Player GameObject with tag 'Player' not found in the scene.");
            enabled = false; 
            return;
        }

        // CameraViewを初期化
        view.Initialize(_player, offset, mapPresenter, cameraToControl);

        // --- ★UIフェードの初期化処理 ---
        // シーンに存在する全てのFadeableUIコンポーネントを自動で検索して取得します。
        // これにより、インスペクターで手動登録する必要がなくなります。
        _allFadeableUis = FindObjectsOfType<FadeableUI>(true);
    }

    void LateUpdate()
    {
        // カメラ位置の更新
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
        if (_player == null || cameraToControl == null || _allFadeableUis == null) return;

        // プレイヤーの3Dワールド座標を、2Dのスクリーン座標に変換します
        Vector2 playerScreenPos = cameraToControl.WorldToScreenPoint(_player.transform.position);

        // キャッシュしておいた全てのUIについて判定
        foreach (var fadeable in _allFadeableUis)
        {
            // UIがnullまたは非アクティブな場合は処理をスキップ
            if (fadeable == null || !fadeable.gameObject.activeInHierarchy) continue;

            // FadeableUIコンポーネントからRectTransformを取得
            RectTransform uiRect = fadeable.GetComponent<RectTransform>();

            // プレイヤーのスクリーン座標がUIの矩形領域内にあるか判定
            // Screen Space - Overlayの場合、第3引数のカメラはnullでOKです
            if (RectTransformUtility.RectangleContainsScreenPoint(uiRect, playerScreenPos, null))
            {
                // 矩形内ならフェードアウト（半透明に）
                fadeable.FadeOut();
            }
            else
            {
                // 矩形外ならフェードイン（元に戻す）
                fadeable.FadeIn();
            }
        }
    }
}