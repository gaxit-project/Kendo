using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace InGame.nonMVP
{
    public class BackGraundChange : MonoBehaviour
    {
        [SerializeField]
        private Renderer targetRenderer; // 対象のレンダラー
    
        [SerializeField, Range(0.0f, 5f)]
        private float duration = 2.0f; // フェードにかかる時間
    
        // マテリアルのインスタンスを保持
        [SerializeField] private Material _materialA;
        [SerializeField] private Material _materialB;
    
        private bool _isFading = false;
        private bool _isSwitchedToB = false; // 現在マテリアルBが表示されているか
        private CancellationTokenSource _cancellationTokenSource;
    
        private void Start()
        {
            
    
            _cancellationTokenSource = new CancellationTokenSource();
        }
    
        private void OnDestroy()
        {
            // オブジェクト破棄時にCancellationTokenをキャンセル・破棄
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
    
            // 作成したマテリアルインスタンスを破棄
            if (_materialA != null) Destroy(_materialA);
            if (_materialB != null) Destroy(_materialB);
        }
    
        // テスト用にキー入力でフェードをトリガー
        private void Update()
        {
            if (GachaManager.Instance.isInvincible)
            {
                targetRenderer.material = _materialB;
            }
            else
            {
                targetRenderer.material = _materialA;
            }
        }
    
        
    }

}
