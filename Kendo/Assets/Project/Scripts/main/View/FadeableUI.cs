using UnityEngine;
using UnityEngine.UI; // Graphicクラスを利用するために必要

/// <summary>
/// UIオブジェクト (Image, RawImage, Panel等) にアタッチし、ディザリングによるフェードを制御します。
/// Graphicコンポーネントを持つオブジェクトで動作します。
/// </summary>
[RequireComponent(typeof(Graphic))] // RendererではなくGraphicを要求
public class FadeableUI : MonoBehaviour
{
    [Header("フェード設定")]
    [Tooltip("半透明時のディザしきい値")]
    [SerializeField] [Range(0f, 1f)] private float fadedThreshold = 0.6f;
    [Tooltip("不透明時のディザしきい値")]
    [SerializeField] [Range(0f, 1f)] private float solidThreshold = 0f;
    [Tooltip("フェード変化の速さ")]
    [SerializeField] private float fadeSpeed = 5f;

    private Graphic _graphic;           // UIの描画コンポーネント (Image, Textなど)
    private Material _materialInstance; // このUI専用のマテリアルインスタンス

    private float _currentTargetThreshold;
    private float _currentThreshold;

    // パフォーマンス向上のため、プロパティ名をIDに変換してキャッシュ
    private static readonly int DitherThreshold = Shader.PropertyToID("_DitherThreshold");

    void Awake()
    {
        // ImageやTextなどのGraphicコンポーネントを取得
        _graphic = GetComponent<Graphic>();
        
        // このUI要素のマテリアルが、他のUIと共有されていない独立したインスタンスであることを保証する
        // これをしないと、同じマテリアルを使っている全てのUIが同時に半透明になってしまう
        if (_graphic.material != null)
        {
            _materialInstance = new Material(_graphic.material);
            _graphic.material = _materialInstance;
        }
        else
        {
            Debug.LogError("FadeableUIを使用するには、対象のUIコンポーネント(Imageなど)にマテリアルが設定されている必要があります。", this);
            enabled = false;
            return;
        }
        
        // 初期状態を不透明に設定
        _currentThreshold = solidThreshold;
        _currentTargetThreshold = solidThreshold;
        _materialInstance.SetFloat(DitherThreshold, _currentThreshold);
    }

    void Update()
    {
        if (_materialInstance == null) return;
        
        // 現在のしきい値が目標値と異なっていれば、滑らかに変化させる
        if (!Mathf.Approximately(_currentThreshold, _currentTargetThreshold))
        {
            _currentThreshold = Mathf.Lerp(_currentThreshold, _currentTargetThreshold, Time.deltaTime * fadeSpeed);
            _materialInstance.SetFloat(DitherThreshold, _currentThreshold);
        }
    }

    /// <summary>
    /// UIを半透明状態にフェードさせます。
    /// </summary>
    public void FadeOut()
    {
        _currentTargetThreshold = fadedThreshold;
    }

    /// <summary>
    /// UIを不透明状態に戻します。
    /// </summary>
    public void FadeIn()
    {
        _currentTargetThreshold = solidThreshold;
    }
}