using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks; // UniTaskを使用するために必要
using System.Threading;       // CancellationTokenを使用するために必要
using Main.View;              // MapViewクラスがこの名前空間にあると仮定
using Main.Model;             // MapModelクラスがこの名前空間にあると仮定

namespace Main.Presenter
{
    /// <summary>
    /// MapModelとIMapView（このコードでは具象クラスMapView）を仲介し、
    /// マップの拡大・縮小ロジックや非同期アニメーション処理を管理するPresenterクラスです。
    /// </summary>
    public class MapPresenter : MonoBehaviour
    {
        [Header("View Dependencies")]
        [Tooltip("マップの描画を担当するMapViewコンポーネント")]
        [SerializeField] private MapView mapView;

        [Header("Line設定 (Viewに渡す初期値)")]
        [Tooltip("線の描画に使用するマテリアル")]
        [SerializeField] private Material lineMaterial;
        [Tooltip("線の色")]
        [SerializeField] private Color lineColor = Color.white;
        [Tooltip("線の太さ")]
        [Range(0.1f, 2f)]
        [SerializeField] private float lineWidth = 0.2f;
        
        [Header("Map設定 (Modelに渡す初期値と挙動設定)")]
        [Tooltip("マップの初期サイズ（原点から辺までの距離）")]
        [SerializeField] private float mapSize = 30f; // 変数名をinitialMapSizeからmapSizeに戻しました (ご提示のコードに合わせて)
        [Tooltip("一度の拡大/縮小で変化するサイズ量")]
        [SerializeField] private float sizeChangeStep = 5f;
        [Tooltip("拡大/縮小アニメーションの時間（秒）")]
        [SerializeField] private float sizeChangeDuration = 0.5f;
        [Tooltip("マップの最小許容サイズ")]
        [SerializeField] private float minSizeLimit = 20f;
        [Tooltip("マップの最大許容サイズ")]
        [SerializeField] private float maxSizeLimit = 100f;
    
        private MapModel _mapModel; // マップのデータとロジックを保持するModel
        private CancellationTokenSource _scalingCTS; // サイズ変更アニメーションのキャンセル用トークンソース
        public event Action<float> OnMapSizeUpdated;
    
        /// <summary>
        /// コンポーネント初期化時に呼び出されます。
        /// Modelの生成、イベント購読、Viewの初期化、および初回マップ描画を行います。
        /// </summary>
        void Start()
        {
            // Modelを初期化設定で生成
            _mapModel = new MapModel(mapSize, minSizeLimit, maxSizeLimit, sizeChangeStep, sizeChangeDuration);
            // Modelのサイズ変更イベントを購読
            _mapModel.OnSizeChanged += HandleModelSizeChanged;

            // Viewの参照を確認
            if (mapView == null)
            {
                Debug.LogError("MapView is not assigned in the Inspector for MapPresenter.");
                enabled = false; // Presenterの動作を停止
                return;
            }
            // Viewを初期化 (線の材質、色、太さなどを設定)
            mapView.InitializeView(lineMaterial, lineColor, lineWidth);

            // Modelの現在のサイズで初回マップ描画を実行
            HandleModelSizeChanged(_mapModel.GetCurrentSize());
        }
    
        /// <summary>
        /// GameObjectが破棄される際に呼び出されます。
        /// 実行中のアニメーションをキャンセルし、イベント購読を解除します。
        /// </summary>
        void OnDestroy()
        {
            CancelScalingAnimation(); // 実行中のアニメーションがあればキャンセル
            _scalingCTS?.Dispose();   // CancellationTokenSourceを破棄

            // Modelのイベント購読を解除
            if (_mapModel != null)
            {
                _mapModel.OnSizeChanged -= HandleModelSizeChanged;
            }
        }

        /// <summary>
        /// MapModelのサイズが変更されたときに呼び出されるイベントハンドラです。
        /// 新しいサイズに基づいてViewの表示（線と壁コライダー）を更新します。
        /// </summary>
        /// <param name="newSize">Modelから通知された新しいマップサイズ。</param>
        private void HandleModelSizeChanged(float newSize)
        {
            // 新しいサイズからマップの四隅の座標を計算
            List<Vector3> corners = CalculateCorners(newSize);
            // Viewに線の表示更新を指示
            mapView.UpdateLineDisplay(corners);
            // Viewに壁コライダーの更新を指示
            mapView.UpdateWallColliders(corners, lineWidth);
            OnMapSizeUpdated?.Invoke(newSize);
        }

        /// <summary>
        /// 指定されたサイズ（原点から辺までの距離）に基づいて、
        /// 正方形マップの四隅のワールド座標リストを計算します。
        /// </summary>
        /// <param name="size">マップの原点から辺までの距離。</param>
        /// <returns>マップの四隅の座標リスト。</returns>
        private List<Vector3> CalculateCorners(float size)
        {
            return new List<Vector3>
            {
                new Vector3(-size, 0, size), // 左上
                new Vector3(size, 0, size),  // 右上
                new Vector3(size, 0, -size), // 右下
                new Vector3(-size, 0, -size) // 左下
            };
        }
            
        /// <summary>
        /// 現在のマップサイズ（原点から辺までの距離）を取得します。
        /// このメソッドはModelから最新のサイズ情報を取得して返します。
        /// </summary>
        /// <returns>現在のマップサイズ。</returns>
        public float GetCurrentMapSize()
        {
            return _mapModel.GetCurrentSize();
        }
    
        /// <summary>
        /// マップを段階的に拡大する非同期処理を開始します。
        /// 実行中のサイズ変更アニメーションがあればキャンセルし、新しいアニメーションを開始します。
        /// </summary>
        public async UniTaskVoid ExpandMap()
        {
            CancelScalingAnimation(); // 既存のアニメーションをキャンセル
            _scalingCTS = new CancellationTokenSource(); // 新しいキャンセルトークンソースを作成
            
            // Modelに次の目標拡大サイズを問い合わせる
            float targetSize = _mapModel.CalculateTargetExpandSize(); 

            // 現在のサイズより大きい場合のみアニメーションを実行
            if (targetSize > _mapModel.GetCurrentSize())
            {
                // Modelからアニメーションのデュレーションを取得し、サイズ変更アニメーションを開始
                await AnimateMapSizeAsync(targetSize, _mapModel.GetSizeChangeDuration(), _scalingCTS.Token);
            }
        }
    
        /// <summary>
        /// マップを段階的に縮小する非同期処理を開始します。
        /// 実行中のサイズ変更アニメーションがあればキャンセルし、新しいアニメーションを開始します。
        /// </summary>
        public async UniTaskVoid ShrinkMap()
        {
            CancelScalingAnimation();
            _scalingCTS = new CancellationTokenSource();
            
            float targetSize = _mapModel.CalculateTargetShrinkSize();

            if (targetSize < _mapModel.GetCurrentSize())
            {
                await AnimateMapSizeAsync(targetSize, _mapModel.GetSizeChangeDuration(), _scalingCTS.Token);
            }
        }
        
        /// <summary>
        /// 実行中のマップサイズ変更アニメーションをキャンセルし、関連リソースを解放します。
        /// </summary>
        private void CancelScalingAnimation()
        {
            if (_scalingCTS != null)
            {
                _scalingCTS.Cancel();   // タスクにキャンセルを要求
                _scalingCTS.Dispose();  // トークンソースを破棄
                _scalingCTS = null;     // 参照をクリア
            }
        }
    
        /// <summary>
        /// マップを指定された目標サイズまで、指定時間をかけて滑らかに変更する非同期アニメーションを実行します。
        /// アニメーション中はViewの表示を直接更新し、完了時にModelの状態を最終的な目標値に設定します。
        /// </summary>
        /// <param name="targetSize">アニメーションの目標マップサイズ。</param>
        /// <param name="duration">アニメーションにかかる時間（秒）。</param>
        /// <param name="cancellationToken">アニメーションをキャンセルするためのトークン。</param>
        private async UniTask AnimateMapSizeAsync(float targetSize, float duration, CancellationToken cancellationToken)
        {
            // アニメーション開始時点でのModelが保持するサイズを取得
            float animationStartModelSize = _mapModel.GetCurrentSize();
            // アニメーション中にViewに表示するための現在サイズ（初期値は開始時サイズ）
            float currentDisplaySize = animationStartModelSize;    
            
            float startTime = Time.time; // アニメーション開始時刻
            float elapsedTime = 0f;      // 経過時間
    
            try
            {
                // 指定されたデュレーションに達するまでループ
                while (elapsedTime < duration)
                {
                    // キャンセルが要求されていればOperationCanceledExceptionをスローして終了
                    cancellationToken.ThrowIfCancellationRequested();
    
                    elapsedTime = Time.time - startTime; // 経過時間を更新
                    float percent = Mathf.Clamp01(elapsedTime / duration); // 進行割合を計算 (0.0～1.0)
                    // 現在の表示サイズを線形補間で計算
                    currentDisplaySize = Mathf.Lerp(animationStartModelSize, targetSize, percent);
                    
                    // 計算した表示サイズでマップの四隅を再計算
                    List<Vector3> corners = CalculateCorners(currentDisplaySize);
                    // Viewの表示を更新（線とコライダー）
                    mapView.UpdateLineDisplay(corners);
                    mapView.UpdateWallColliders(corners, lineWidth);
    
                    // 次のフレームまで待機 (UniTaskの機能で、キャンセルも監視)
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
                // アニメーション完了後、Modelのサイズを最終的な目標値に設定
                // これによりOnSizeChangedイベントが発行され、HandleModelSizeChangedが呼び出されて
                // ViewがModelの最終状態と同期される。
                _mapModel.SetCurrentSize(targetSize); 
            }
            catch (System.OperationCanceledException)
            {
                // アニメーションがキャンセルされた場合
                // Modelの現在の（キャンセル時点での）サイズでViewを再描画して整合性を保つ
                HandleModelSizeChanged(_mapModel.GetCurrentSize());
            }
            catch (System.Exception ex)
            {
                // その他の予期せぬエラーが発生した場合
                Debug.LogError($"Error during map size animation: {ex.Message}");
                // 安全のため、Modelのサイズをアニメーション開始前の状態に戻す
                _mapModel.SetCurrentSize(animationStartModelSize);
            }
        }
    }
}