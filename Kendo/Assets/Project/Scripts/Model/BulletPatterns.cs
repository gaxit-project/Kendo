using UnityEngine;

/// <summary>
/// 弾の発射パターンライブラリ（全てBulletSpawner.SpawnBulletを利用）
/// </summary>
public static class BulletPatterns
{
    /// <summary>単発：プレイヤーに向かって発射</summary>
    public static void ShootAt(Vector3 spawnPos, Vector3 targetPos, float speed)
    {
        Vector3 dir = (targetPos - spawnPos).normalized;
        BulletSpawner.SpawnBullet(spawnPos, dir * speed);
    }

    /// <summary>3Way弾：中心と左右±angleで扇状に3方向発射</summary>
    public static void Shoot3Way(Vector3 spawnPos, Vector3 targetPos, float speed, float angle)
    {
        Vector3 baseDir = (targetPos - spawnPos).normalized;
        for (int i = -1; i <= 1; i++)
        {
            Vector3 rotatedDir = Quaternion.Euler(0, angle * i, 0) * baseDir;
            BulletSpawner.SpawnBullet(spawnPos, rotatedDir * speed);
        }
    }

    /// <summary>扇型弾（任意本数）</summary>
    public static void ShootFan(Vector3 spawnPos, float startAngle, float totalAngle, int count, float speed)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (totalAngle / (count - 1)) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.right;
            BulletSpawner.SpawnBullet(spawnPos, dir.normalized * speed);
        }
    }

    /// <summary>追尾弾（Velocityは初期のみ。追尾処理は弾側Updateで実装）</summary>
    public static void ShootHoming(Vector3 spawnPos, float speed, Transform target)
    {
        Vector3 dir = (target.position - spawnPos).normalized;
        BulletSpawner.SpawnBullet(spawnPos, dir * speed);
    }

    /// <summary>ランダムばら撒き弾</summary>
    public static void ShootRandomSpread(Vector3 spawnPos, float speed, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 rand2D = Random.insideUnitCircle.normalized;
            Vector3 dir = new Vector3(rand2D.x, 0, rand2D.y);
            BulletSpawner.SpawnBullet(spawnPos, dir * speed);
        }
    }

    /// <summary>花びらスピン弾（Velocityを時間で回す）</summary>
    public static void ShootSpiral(Vector3 spawnPos, float baseAngle, float spinOffset, int count, float speed)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = baseAngle + spinOffset * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.right;
            BulletSpawner.SpawnBullet(spawnPos, dir.normalized * speed);
        }
    }

    /// <summary>横から来る弾</summary>
    public static void ShootSide(Vector3 spawnPos, float speed, bool fromRight)
    {
        Vector3 dir = fromRight ? Vector3.left : Vector3.right;
        BulletSpawner.SpawnBullet(spawnPos, dir * speed);
    }

    /// <summary>円運動する弾を扇状に配置して発射</summary>
    public static void ShootCircularFan(Vector3 center, float radius, int count, float startAngle, float angleStep, float angularSpeed)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 spawnPos = center + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;

            BulletSpawner.SpawnBullet(
                position: spawnPos,
                velocity: Vector3.zero,
                isCircular: true,
                center: center,
                radius: radius,
                startAngleDeg: angle,
                angularSpeedDeg: angularSpeed
            );
        }
    }

    /// <summary>円運動弾を円形に等間隔配置して全周回転</summary>
    public static void ShootSpinningCircle(Vector3 center, float radius, int count, float angularSpeed)
    {
        ShootCircularFan(center, radius, count, 0f, 360f / count, angularSpeed);
    }

    /// <summary>回転しながら放射的に飛ぶスパイラル弾</summary>
    public static void ShootSpiralExpanding(
        Vector3 center,
        float initialRadius,
        int count,
        float startAngle,
        float angleStep,
        float angularSpeed,
        float radiusGrowthPerSec,
        float speed)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            // 初期の弾の位置は中心から初期半径の距離で配置
            Vector3 spawnPos = center + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * initialRadius;

            BulletSpawner.SpawnBullet(
                position: spawnPos,
                velocity: Vector3.zero,
                isCircular: true,
                center: center,
                radius: initialRadius,
                startAngleDeg: angle,
                angularSpeedDeg: angularSpeed,
                radiusGrowthPerSec: radiusGrowthPerSec
            );
        }
    }


    
    /// <summary>時間経過で回転しながら半径も増えていく真のスパイラル弾を発射</summary>
    public static void ShootTrueSpiral(
        Vector3 center,
        float initialRadius,
        int count,
        float startAngle,
        float angleStep,
        float angularSpeed,
        float radiusGrowthPerSec)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            // 初期位置は半径initialRadiusで円周上に配置
            Vector3 spawnPos = center + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * initialRadius;

            BulletSpawner.SpawnBullet(
                position: spawnPos,
                velocity: Vector3.zero,
                isCircular: true,
                center: center,
                radius: initialRadius,
                startAngleDeg: angle,
                angularSpeedDeg: angularSpeed,
                radiusGrowthPerSec: radiusGrowthPerSec,
                isTrueSpiral: true // 真のスパイラルフラグ
            );
        }
    
    }
    


}
