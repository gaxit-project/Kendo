using System;
using UnityEngine; // Mathf を使用するため

namespace Main.Model
{
    /// <summary>
    /// マップのデータと状態ロジックを管理します。
    /// 変数アクセスはアクセサメソッドを通じて行います。
    /// </summary>
    public class MapModel
    {
        private float _currentSize;
        private readonly float _initialSize;
        private readonly float _minSizeLimit;
        private readonly float _maxSizeLimit;
        private readonly float _sizeChangeStep;
        private readonly float _sizeChangeDuration;

        /// <summary>
        /// マップサイズが変更されたときに発行されるイベント。新しいサイズを引数とします。
        /// </summary>
        public event Action<float> OnSizeChanged;

        public MapModel(float initialSize, float minSize, float maxSize, float step, float duration)
        {
            _initialSize = initialSize;
            _minSizeLimit = minSize;
            _maxSizeLimit = maxSize;
            _sizeChangeStep = step;
            _sizeChangeDuration = duration;
            _currentSize = Mathf.Clamp(initialSize, _minSizeLimit, _maxSizeLimit);
        }

        #region アクセスメソッド

        /// <summary>
        /// 現在のマップサイズを取得します。
        /// </summary>
        public float GetCurrentSize() => _currentSize;
        
        /// <summary>
        /// 初期マップサイズを取得します。
        /// </summary>
        public float GetInitialSize() => _initialSize;
        
        /// <summary>
        /// 最小許容マップサイズを取得します。
        /// </summary>
        public float GetMinSizeLimit() => _minSizeLimit;
        
        /// <summary>
        /// 最大許容マップサイズを取得します。
        /// </summary>
        public float GetMaxSizeLimit() => _maxSizeLimit;
        
        /// <summary>
        /// サイズ変更時のステップ量を取得します。
        /// </summary>
        public float GetSizeChangeStep() => _sizeChangeStep;
        
        /// <summary>
        /// サイズ変更アニメーションのデュレーションを取得します。
        /// </summary>
        public float GetSizeChangeDuration() => _sizeChangeDuration;

        #endregion
        

        /// <summary>
        /// 現在のマップサイズを設定します。変更があればOnSizeChangedイベントを発行します。
        /// </summary>
        /// <param name="newSize">新しいマップサイズ。</param>
        public void SetCurrentSize(float newSize)
        {
            float clampedSize = Mathf.Clamp(newSize, _minSizeLimit, _maxSizeLimit);
            if (!Mathf.Approximately(_currentSize, clampedSize))
            {
                _currentSize = clampedSize;
                OnSizeChanged?.Invoke(_currentSize);
            }
        }

        /// <summary>
        /// 次の拡大操作での目標サイズを計算します。
        /// </summary>
        /// <returns>目標サイズ。</returns>
        public float CalculateTargetExpandSize()
        {
            return Mathf.Min(_currentSize + _sizeChangeStep, _maxSizeLimit);
        }

        /// <summary>
        /// 次の縮小操作での目標サイズを計算します。
        /// </summary>
        /// <returns>目標サイズ。</returns>
        public float CalculateTargetShrinkSize()
        {
            return Mathf.Max(_currentSize - _sizeChangeStep, _minSizeLimit);
        }
    }
}