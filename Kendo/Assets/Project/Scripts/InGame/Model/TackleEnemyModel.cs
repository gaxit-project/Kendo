using UnityEngine;
using InGame.Model;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TackleEnemyModel : IEnemyModel
{
    /// <summary>
    /// 突進AIの内部状態
    /// </summary>
    private enum AIState
    {
        Idle,       // 索敵中
        Tracking    // プレイヤーを追跡中
    }

    // --- AI状態 ---
    private AIState _currentState = AIState.Idle;

    // --- 突進型固有のパラメータ ---
    private readonly float _attackSpan;
    private readonly float _tackleForce;
    private readonly float _detectionAngle;
    private readonly float _detectionRadius;

    // --- 内部状態変数 ---
    private float _attackTimer;

    // --- IEnemyModelから引き継ぐ共通パラメータ ---
    private readonly float _mass;
    private readonly float _restitution;
    private readonly float _drag;
    private readonly float _stopThreshold;
    private readonly int _maxBounceCount;
    private readonly float _wallCheckRadius;
    private readonly float _wallCheckDistance;
    private readonly MobController _mob;
    
    public TackleEnemyModel(float attackSpan, float tackleForce, float detectionAngle, float detectionRadius, float mass, float restitution, float drag, float stopThreshold, int maxBounceCount, float wallCheckRadius, float wallCheckDistance,  MobController mob)
    {
        _attackSpan = attackSpan;
        _tackleForce = tackleForce;
        _detectionAngle = detectionAngle;
        _detectionRadius = detectionRadius;
        _mass = mass;
        _restitution = restitution;
        _drag = drag;
        _stopThreshold = stopThreshold;
        _maxBounceCount = maxBounceCount;
        _wallCheckRadius = wallCheckRadius;
        _wallCheckDistance = wallCheckDistance;

        _mob = mob;
    }

    /// <summary>
    /// MobManagerから毎フレーム呼び出され、AIの更新を行う。
    /// </summary>
    public void UpdateAttackState(float timer)
    {
        if (_mob == null) return;
        
        
        Transform playerTransform = MobManager.Instance.GetPlayerTransform();
        if (playerTransform == null)
        {
            // プレイヤーがいない場合、索敵状態に戻す
            _currentState = AIState.Idle;
            return;
        }

        // 状態に応じて処理を分岐
        if (_currentState == AIState.Idle)
        {
            // --- 索敵中の処理 ---
            if (IsPlayerInDetectionRange(_mob.transform, playerTransform))
            {
                // プレイヤーを発見したら追跡状態に移行
                _currentState = AIState.Tracking;
                _attackTimer = 0f; // 追跡開始時にタイマーリセット
            }
        }
        else // _currentState == AIState.Tracking
        {
            // --- 追跡中の処理 ---
            if (!IsPlayerInDetectionRange(_mob.transform, playerTransform))
            {
                // プレイヤーを見失ったら索敵状態に戻る
                _currentState = AIState.Idle;
                return;
            }

            // プレイヤーの方向を向く
            Vector3 lookPos = playerTransform.position;
            lookPos.y = _mob.transform.position.y;
            _mob.transform.LookAt(lookPos);
            
        }
    }
    
    /// <summary>
    /// 突進攻撃を実行する
    /// </summary>
    public async UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken)
    {
        if (mob.GetIsKnockback() || cancellationToken.IsCancellationRequested) return;

        if (AIState.Idle == _currentState) return;

        // MobControllerに突進の実行を依頼
        mob.PerformTackle(_tackleForce);

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// プレイヤーが索敵範囲内にいるか判定する
    /// </summary>
    private bool IsPlayerInDetectionRange(Transform mobTransform, Transform playerTransform)
    {
        Vector3 directionToPlayer = playerTransform.position - mobTransform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 距離が索敵範囲外ならfalse
        if (distanceToPlayer > _detectionRadius)
        {
            return false;
        }

        // 視野角のチェック
        float angleToPlayer = Vector3.Angle(mobTransform.forward, directionToPlayer);
        
        // 視野角の半分より角度が小さければ範囲内
        return angleToPlayer < _detectionAngle / 2f;
    }
    
    
    

    // --- 以下、IEnemyModelインターフェースの未使用または単純な実装 ---
    public float GetAttackSpan() => _attackSpan;
    public void PerformShot(Transform mobTransform) { }
    public void FanShot(Transform mobTransform) { }
    public void SuperShot(Transform mobTransform) { }
    public void RevertToNormalAttackState() { }
    public bool GetHasPerformedSpecialAttack() => false;
    public void SetHasPerformedSpecialAttack(bool value) { }
    
    public float GetMass() => _mass;
    public float GetRestitution() => _restitution;
    public float GetDrag() => _drag;
    public float GetStopThreshold() => _stopThreshold;
    public int GetMaxBounceCount() => _maxBounceCount;
    public float GetWallCheckRadius() => _wallCheckRadius;
    public float GetWallCheckDistance() => _wallCheckDistance;
}