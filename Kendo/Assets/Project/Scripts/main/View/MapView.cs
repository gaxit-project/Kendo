using System.Collections.Generic;
using UnityEngine;

namespace Main.View
{
    /// <summary>
    /// マップの視覚的表現（線と壁コライダー）を担当するMonoBehaviour。
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class MapView : MonoBehaviour, IMapView
    {
        private LineRenderer _lineRenderer;
        private readonly List<GameObject> _wallObjects = new List<GameObject>();

        // View固有の設定はInitializeViewでPresenterから受け取る
        private Material _lineMaterial;
        private Color _lineColor;
        private float _lineWidth;

        /// <summary>
        /// IMapViewインターフェースのInitializeViewメソッド。
        /// Presenterから呼び出され、Viewの初期設定を行います。
        /// </summary>
        public void InitializeView(Material material, Color color, float lineWidth)
        {
            _lineMaterial = material;
            _lineColor = color;
            _lineWidth = lineWidth;

            _lineRenderer = GetComponent<LineRenderer>();
            // LineRendererの基本設定
            _lineRenderer.material = _lineMaterial;
            _lineRenderer.startColor = _lineColor;
            _lineRenderer.endColor = _lineColor;
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.loop = true;
        }

        /// <summary>
        /// IMapViewインターフェースのUpdateLineDisplayメソッド。
        /// マップの外枠線表示を更新します。
        /// </summary>
        public void UpdateLineDisplay(List<Vector3> corners)
        {
            if (_lineRenderer == null) return;
            if (corners == null)
            {
                _lineRenderer.positionCount = 0;
                return;
            }
            _lineRenderer.positionCount = corners.Count;
            _lineRenderer.SetPositions(corners.ToArray());
        }

        /// <summary>
        /// IMapViewインターフェースのUpdateWallCollidersメソッド。
        /// マップの辺に沿って壁コライダーを生成または更新します。
        /// </summary>
        public void UpdateWallColliders(List<Vector3> corners, float colliderLineWidth)
        {
            foreach (GameObject wall in _wallObjects)
            {
                if (wall != null) Destroy(wall);
            }
            _wallObjects.Clear();

            if (corners == null || corners.Count < 2) return;

            for (int i = 0; i < corners.Count; i++)
            {
                Vector3 startPoint = corners[i];
                Vector3 endPoint = corners[(i + 1) % corners.Count];

                GameObject wallObj = new GameObject($"Wall_{i}");
                wallObj.transform.SetParent(this.transform); // MapViewのGameObjectの子にする
                wallObj.tag = "Wall";

                Vector3 segmentDirection = endPoint - startPoint;
                wallObj.transform.position = startPoint + segmentDirection / 2;

                if (segmentDirection.sqrMagnitude > Mathf.Epsilon)
                {
                    wallObj.transform.rotation = Quaternion.LookRotation(segmentDirection.normalized, Vector3.up);
                }

                BoxCollider wallCollider = wallObj.AddComponent<BoxCollider>();
                wallCollider.size = new Vector3(colliderLineWidth, 2f, segmentDirection.magnitude);
                _wallObjects.Add(wallObj);
            }
        }
        
        /// <summary>
        /// IMapViewインターフェースのGetGameObjectメソッド。
        /// </summary>
        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}