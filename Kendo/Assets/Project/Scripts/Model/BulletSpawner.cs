using UnityEngine;

/// <summary>
/// BulletManagerのSpawnBulletを呼び出す静的クラス
/// </summary>
public static class BulletSpawner
{
    /// <summary>
    /// 弾の生成を委譲する静的メソッド
    /// </summary>
    public static void SpawnBullet(
        Vector3 position,
        Vector3 velocity,
        bool isCircular = false,
        Vector3 center = default,
        float radius = 0f,
        float startAngleDeg = 0f,
        float angularSpeedDeg = 0f,
        float radiusGrowthPerSec = 0f,
        bool isTrueSpiral = false )
    {
        if (BulletManager.Instance != null)
        {
            BulletManager.Instance.SpawnBullet(
                position,
                velocity,
                isCircular,
                center,
                radius,
                startAngleDeg,
                angularSpeedDeg,
                radiusGrowthPerSec);
        }
        else
        {
            Debug.LogWarning("BulletManager.Instance is null.");
        }
    }
}
