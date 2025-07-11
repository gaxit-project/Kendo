using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace InGame.Model
{
    public class EnemyModel : IEnemyModel
    {
        // Mob固有のプロパティ
        private float _attackSpan;
        private float _mass;
        private float _restitution;
        private float _drag;
        private float _stopThreshold;
        private int _maxBounceCount;
        private float _wallCheckRadius;
        private float _wallCheckDistance;
        private bool _hasPerformedSpecialAttack;

        private AttackState _currentAttackState;
        private int _attackPhase;

        public EnemyModel(float attackSpan, float mass, float restitution, float drag, float stopThreshold, int maxBounceCount, float wallCheckRadius, float wallCheckDistance)
        {
            _attackSpan = attackSpan;
            _mass = mass;
            _restitution = restitution;
            _drag = drag;
            _stopThreshold = stopThreshold;
            _maxBounceCount = maxBounceCount;
            _wallCheckRadius = wallCheckRadius;
            _wallCheckDistance = wallCheckDistance;
            
            _currentAttackState = new SingleShotState();
            _attackPhase = 1;
            _hasPerformedSpecialAttack = false;
        }

        // アクセサメソッド
        public float GetAttackSpan() => _attackSpan;
        public float GetMass() => _mass;
        public float GetRestitution() => _restitution;
        public float GetDrag() => _drag;
        public float GetStopThreshold() => _stopThreshold;
        public int GetMaxBounceCount() => _maxBounceCount;
        public float GetWallCheckRadius() => _wallCheckRadius;
        public float GetWallCheckDistance() => _wallCheckDistance;
        public bool GetHasPerformedSpecialAttack() => _hasPerformedSpecialAttack;
        public void SetHasPerformedSpecialAttack(bool value) => _hasPerformedSpecialAttack = value;
        
        // 攻撃ロジック
        public void UpdateAttackState(float timer)
        {
            if (_currentAttackState is SpecialAttackState) return;

            if (timer > MobManager.Instance.GetMobStateInterval2() && _attackPhase < 3)
            {
                _currentAttackState = new TripleShotState();
                _attackPhase = 3;
            }
            else if (timer > MobManager.Instance.GetMobStateInterval1() && _attackPhase < 2)
            {
                _currentAttackState = new DoubleShotState();
                _attackPhase = 2;
            }
        }

        public UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken)
        {
            return _currentAttackState.ExecuteAttack(mob, cancellationToken);
        }

        public void PerformShot(Transform mobTransform)
        {
            BulletManager.Instance.SpawnBullet(mobTransform.position, mobTransform.forward.normalized * 8f);
            SoundSE.Instance?.Play("Shot");
        }

        public void FanShot(Transform mobTransform)
        {
            int fanBulletCount = 5;
            float fanTotalAngle = 90f;
            float fanBulletSpeed = 8f;
            
            float centerAngle = Vector3.SignedAngle(Vector3.right, mobTransform.forward, Vector3.up);
            float startAngle = centerAngle - (fanTotalAngle / 2f);

            BulletPatterns.ShootFan(mobTransform.position, startAngle, fanTotalAngle, fanBulletCount, fanBulletSpeed);
            SoundSE.Instance?.Play("Shot");
        }
        
        public void SuperShot(Transform mobTransform)
        {
            BulletPatterns.ShootRandomSpread(mobTransform.position, 8f, 20);
            BulletManager.Instance.SpawnBullet(mobTransform.position, mobTransform.forward.normalized * 8f);
            SoundSE.Instance?.Play("Shot");
        }
        
        public void RevertToNormalAttackState()
        {
            _attackPhase = 0;
            float timer = ScoreManager.Instance.GetMinutes() * 60 + ScoreManager.Instance.GetSeconds();
            UpdateAttackState(timer);
        }
    }
    
    
    
    /// <summary>
    /// 攻撃ステートの振る舞いを定義する抽象基底クラス
    /// </summary>
    public abstract class AttackState
    {
        protected float burstInterval = 0.4f; // 連射間隔

        /// <summary>
        /// 攻撃を実行する非同期メソッド
        /// </summary>
        public abstract UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken);
    }

    /// <summary>
    /// 単発攻撃ステート
    /// </summary>
    public class SingleShotState : AttackState
    {
        public override async UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken)
        {
            if (mob.GetIsKnockback() || cancellationToken.IsCancellationRequested) return;
            mob.PerformShot();
            await UniTask.CompletedTask;
        }
    }

    /// <summary>
    /// 2連射攻撃ステート
    /// </summary>
    public class DoubleShotState : AttackState
    {
        public override async UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken)
        {
            for (int i = 0; i < 2; i++)
            {
                if (mob.GetIsKnockback() || cancellationToken.IsCancellationRequested) return;
                mob.PerformShot();
                await UniTask.Delay(TimeSpan.FromSeconds(burstInterval), cancellationToken: cancellationToken);
            }
        }
    }

    /// <summary>
    /// 3連射攻撃ステート
    /// </summary>
    public class TripleShotState : AttackState
    {
        public override async UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken)
        {
            if (mob.GetIsKnockback() || cancellationToken.IsCancellationRequested) return;
            mob.FanShot();
            await UniTask.Delay(TimeSpan.FromSeconds(burstInterval), cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// 1回限りの特殊攻撃ステート
    /// </summary>
    public class SpecialAttackState : AttackState
    {
        public override async UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken)
        {
            if (mob.GetIsKnockback() || cancellationToken.IsCancellationRequested) return;
            mob.SuperShot();
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: cancellationToken);
            mob.RevertToNormalAttackState();
        }
    }
    
    
}