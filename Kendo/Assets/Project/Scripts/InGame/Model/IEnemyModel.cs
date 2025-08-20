using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public interface IEnemyModel
{
    // プロパティのアクセサメソッド
    float GetAttackSpan();
    float GetMass();
    float GetRestitution();
    float GetDrag();
    float GetStopThreshold();
    int GetMaxBounceCount();
    float GetWallCheckRadius();
    float GetWallCheckDistance();
    bool GetHasPerformedSpecialAttack();
    void SetHasPerformedSpecialAttack(bool value);

    // 処理メソッド
    void UpdateAttackState(float timer);
    
    
    UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken);
    void PerformShot(Transform mobTransform);
    void FanShot(Transform mobTransform);
    void SuperShot(Transform mobTransform);
    void RevertToNormalAttackState();
}