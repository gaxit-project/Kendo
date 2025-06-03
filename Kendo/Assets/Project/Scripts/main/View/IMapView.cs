using System.Collections.Generic;
using UnityEngine;

namespace Main.View // または Main.View など適切な名前空間
{
    /// <summary>
    /// マップ表示に関するビューのインターフェース。
    /// </summary>
    public interface IMapView
    {
        /// <summary>
        /// ビューを初期化します。
        /// </summary>
        void InitializeView(Material material, Color color, float lineWidth);

        /// <summary>
        /// マップの外枠線表示を更新します。
        /// </summary>
        /// <param name="corners">マップの四隅の座標リスト。</param>
        void UpdateLineDisplay(List<Vector3> corners);

        /// <summary>
        /// 壁コライダーを更新します。
        /// </summary>
        /// <param name="corners">マップの四隅の座標リスト。</param>
        /// <param name="colliderLineWidth">コライダーの基準となる線の太さ。</param>
        void UpdateWallColliders(List<Vector3> corners, float colliderLineWidth);

        /// <summary>
        /// ViewがアタッチされているGameObjectを取得します。
        /// </summary>
        GameObject GetGameObject();
    }
}