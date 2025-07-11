using UnityEngine;
using Main.Presenter; // MapPresenterクラスの名前空間を指定してください

/// <summary>
/// デバッグ用にキー入力でマップの拡大・縮小を制御するスクリプトです。
/// </summary>
public class DebugScript : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("マップの拡大・縮小を制御するMapPresenterコンポーネントへの参照")]
    [SerializeField] private MapPresenter mapPresenter;

    /// <summary>
    /// スクリプト初期化時に呼び出されます。
    /// MapPresenterへの参照が設定されていない場合、シーン内から検索を試みます。
    /// </summary>
    void Start()
    {
        // InspectorでmapPresenterが設定されていない場合、シーン内から検索する
        if (mapPresenter == null)
        {
            mapPresenter = FindObjectOfType<MapPresenter>();
            if (mapPresenter == null)
            {
                Debug.LogError("[DebugScript] MapPresenterのインスタンスがシーンに見つかりません。マップ操作機能は無効です。");
                enabled = false; // MapPresenterが見つからない場合はこのスクリプトを無効化
            }
            else
            {
                Debug.Log("[DebugScript] MapPresenterをシーンから見つけました。");
            }
        }
    }

    /// <summary>
    /// フレームごとに呼び出されます。
    /// 上下矢印キーの入力を検出し、マップの拡大・縮小を要求します。
    /// </summary>
    void Update()
    {
        // mapPresenterが利用可能でない場合は何もしない
        if (mapPresenter == null || !mapPresenter.isActiveAndEnabled)
        {
            return;
        }

        // 上矢印キーが押されたらマップを拡大
        if (Input.GetKeyDown("p"))
        {
            mapPresenter.ExpandMap();
            Debug.Log("Up Arrow pressed: Requesting map expansion.");
        }

        // 下矢印キーが押されたらマップを縮小
        if (Input.GetKeyDown("o"))
        {
            mapPresenter.ShrinkMap();
            Debug.Log("Down Arrow pressed: Requesting map shrinkage.");
        }
    }
}