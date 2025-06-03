using UnityEngine;
using Main.Presenter; // MapPresenterの名前空間を適切に指定してください
// CameraViewの名前空間も必要に応じてusingに追加 (同じ名前空間なら不要)
// using Main.View; 

/// <summary>
/// CameraViewの初期化と更新呼び出しを管理します。
/// 必要な依存関係（プレイヤー、マップ情報、制御カメラ）をCameraViewに提供します。
/// </summary>
public class CameraPresenter : MonoBehaviour
{
    private GameObject _player; // 追従対象のプレイヤー

    [Header("Camera Settings")]
    [Tooltip("プレイヤーに対するカメラのY軸オフセットなど、基本的なオフセット値")]
    [SerializeField] private Vector3 offset = new Vector3(0, 30, 0); 

    [Header("Dependencies")]
    [Tooltip("カメラの表示ロジックを担当するCameraViewコンポーネント")]
    [SerializeField] private CameraView view; 
    [Tooltip("マップ情報を提供するMapPresenterコンポーネント")]
    [SerializeField] private MapPresenter mapPresenter; 
    [Tooltip("このPresenterが制御するCameraコンポーネント")]
    [SerializeField] private Camera cameraToControl; 

    /// <summary>
    /// コンポーネント初期化（Startより前）のタイミングで依存関係のチェックを行います。
    /// </summary>
    void Awake()
    {
        // Inspectorで設定されるべき依存関係のnullチェック
        if (view == null)
        {
            Debug.LogError("[CameraPresenter] CameraView component is not assigned in the Inspector.");
            enabled = false; // Presenterを無効化
            return;
        }

        if (mapPresenter == null)
        {
            // シーンから検索するフォールバック処理
            mapPresenter = FindObjectOfType<MapPresenter>();
            if (mapPresenter == null)
            {
                Debug.LogError("[CameraPresenter] MapPresenter is not assigned and could not be found in the scene.");
                enabled = false;
                return;
            }
            Debug.LogWarning("[CameraPresenter] MapPresenter was not assigned in Inspector, found via FindObjectOfType.");
        }

        if (cameraToControl == null)
        {
            // 自身にアタッチされたCameraコンポーネントを探すか、メインカメラを試す
            cameraToControl = GetComponent<Camera>();
            if (cameraToControl == null) cameraToControl = Camera.main;

            if (cameraToControl == null)
            {
                Debug.LogError("[CameraPresenter] CameraToControl is not assigned and no Camera component found on this GameObject or as Camera.main.");
                enabled = false;
                return;
            }
            Debug.LogWarning("[CameraPresenter] CameraToControl was not assigned, using found Camera component or Camera.main as fallback.");
        }
    }

    /// <summary>
    /// ゲーム開始時にプレイヤーを検索し、CameraViewを初期化します。
    /// </summary>
    void Start()
    {
        // プレイヤーをタグで検索 (より堅牢)
        _player = GameObject.FindWithTag("Player"); 
        if (_player == null)
        {
            Debug.LogError("[CameraPresenter] Player GameObject with tag 'Player' not found in the scene.");
            enabled = false; // Presenterを無効化
            return;
        }

        // CameraViewを初期化 (Awakeで依存関係はチェック済みと仮定)
        view.Initialize(_player, offset, mapPresenter, cameraToControl);
    }

    /// <summary>
    /// 毎フレームの最後にカメラ位置の更新を指示します。
    /// </summary>
    void LateUpdate()
    {
        // viewが初期化済みで有効な場合のみカメラ位置を更新
        if (view != null && view.enabled) 
        {
            view.SetCameraPosition();
        }
    }
}