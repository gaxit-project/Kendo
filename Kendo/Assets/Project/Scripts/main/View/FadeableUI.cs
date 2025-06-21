using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIオブジェクトをマネージャーに登録し、CameraPresenterからの命令に応じてマテリアルのプロパティを滑らかに変化させます。
/// </summary>
[RequireComponent(typeof(Graphic))]
public class FadeableUI : MonoBehaviour
{
    [Header("マテリアル設定")]
    [Tooltip("フェードさせるベースのマテリアル（ディザ抜きシェーダーを使ったもの）")]
    [SerializeField] private Material _baseMaterial;

    [Header("フェード設定")]
    [Tooltip("半透明時のディザしきい値（0.0でほぼ透明、1.0で不透明）")]
    [Range(0f, 1f)]
    [SerializeField] private float fadedThreshold = 0.3f;

    [Tooltip("フェード変化の速さ")]
    [SerializeField] private float fadeSpeed = 20f;

    private Graphic _graphic;
    private Material _materialInstance;

    private const float SOLID_THRESHOLD = 1.0f;
    private float _currentTargetThreshold;
    private float _currentThreshold;

    private static readonly int DitherThreshold = Shader.PropertyToID("_DitherThreshold");

    // --- マネージャーへの登録/解除 ---
    private void OnEnable()
    {
        // このUIが有効になった時、マネージャーのリストに自分自身を追加する
        if (!FadeableUIManager.Instances.Contains(this))
        {
            FadeableUIManager.Instances.Add(this);
        }
    }

    private void OnDisable()
    {
        // このUIが無効になったり、破棄された時、マネージャーのリストから自分自身を削除する
        if (FadeableUIManager.Instances.Contains(this))
        {
            FadeableUIManager.Instances.Remove(this);
        }
    }

    // --- 初期化処理 ---
    void Awake()
    {
        _graphic = GetComponent<Graphic>();

        if (_baseMaterial == null)
        {
            Debug.LogError("FadeableUIにベースマテリアルが設定されていません！", this);
            enabled = false;
            return;
        }

        _materialInstance = new Material(_baseMaterial);
        _graphic.material = _materialInstance;

        _currentThreshold = SOLID_THRESHOLD;
        _currentTargetThreshold = SOLID_THRESHOLD;
        _materialInstance.SetFloat(DitherThreshold, _currentThreshold);
    }

    // --- プロパティ更新処理 ---
    void Update()
    {
        if (_materialInstance == null) return;

        if (!Mathf.Approximately(_currentThreshold, _currentTargetThreshold))
        {
            _currentThreshold = Mathf.Lerp(_currentThreshold, _currentTargetThreshold, Time.deltaTime * fadeSpeed);
            _materialInstance.SetFloat(DitherThreshold, _currentThreshold);
        }
    }

    // --- 外部からの命令を受け付けるメソッド ---
    public void FadeOut()
    {
        _currentTargetThreshold = fadedThreshold;
    }

    public void FadeIn()
    {
        _currentTargetThreshold = SOLID_THRESHOLD;
    }
}