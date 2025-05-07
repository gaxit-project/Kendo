using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; private set; }

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 1000;

    private List<Bullet> activeBullets = new List<Bullet>();
    private Queue<Bullet> bulletPool = new Queue<Bullet>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 重複するインスタンスを破棄
            return;
        }
        Instance = this;


        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            bulletPool.Enqueue(new Bullet(obj));
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // 全弾処理
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = activeBullets[i];
            bullet.Position += bullet.Velocity * dt;
            bullet.GameObject.transform.position = bullet.Position;

            // 画面外などで消す条件
            if (!IsInScreen(bullet.Position))
            {
                ReleaseBullet(bullet);
                activeBullets.RemoveAt(i);
            }
        }
    }

    public void SpawnBullet(Vector3 position, Vector3 velocity)
    {
        if (bulletPool.Count > 0)
        {
            Bullet bullet = bulletPool.Dequeue();
            bullet.Position = position;
            bullet.Velocity = velocity;
            bullet.GameObject.transform.position = position;
            bullet.GameObject.SetActive(true);
            activeBullets.Add(bullet);
        }
    }

    private void ReleaseBullet(Bullet bullet)
    {
        bullet.GameObject.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    private bool IsInScreen(Vector3 pos)
    {
        return pos.x > -30f && pos.x < 30f && pos.z > -30f && pos.z < 30f; // 適宜調整
    }

    public void ClearAllBullets()
    {
        foreach (var bullet in activeBullets)
        {
            bullet.GameObject.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
        activeBullets.Clear();
    }


    private class Bullet
    {
        public GameObject GameObject { get; private set; }
        public Vector3 Position;
        public Vector3 Velocity;

        // 花弁弾用
        public float AngularVelocityDeg; // 回転弾角速度（毎秒度）

        // 円運動弾用
        public bool IsCircular;
        public Vector3 Center;
        public float Radius;
        public float AngleDeg;
        public float AngularSpeedDeg;

        public Bullet(GameObject obj)
        {
            GameObject = obj;
        }
    }
}



    // 弾のパターン発射ライブラリ

public static class BulletPatterns
{
    // 単発：プレイヤーに向かって発射
    public static void ShootAt(Vector3 spawnPos, Vector3 targetPos, float speed, Action<Vector3, Vector3> fire)
    {
        Vector3 dir = (targetPos - spawnPos).normalized;
        fire(spawnPos, dir * speed);
    }

    // 3Way弾：プレイヤー中心、±angleで扇状に発射
    public static void Shoot3Way(Vector3 spawnPos, Vector3 targetPos, float speed, float angle, Action<Vector3, Vector3> fire)
    {
        Vector3 baseDir = (targetPos - spawnPos).normalized;
        for (int i = -1; i <= 1; i++)
        {
            Vector3 rotatedDir = Quaternion.Euler(0, angle * i, 0) * baseDir;
            fire(spawnPos, rotatedDir * speed);
        }
    }

    // 扇型弾（任意本数）
    public static void ShootFan(Vector3 spawnPos, float startAngle, float totalAngle, int count, float speed, Action<Vector3, Vector3> fire)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (totalAngle / (count - 1)) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.right;
            fire(spawnPos, dir.normalized * speed);
        }
    }

    // 追尾弾：Velocityを毎フレーム補正する（別途追尾処理が必要）
    public static void ShootHoming(Vector3 spawnPos, float speed, Transform target, Action<Vector3, Vector3, Transform> fire)
    {
        Vector3 dir = (target.position - spawnPos).normalized;
        fire(spawnPos, dir * speed, target);
    }

    // ランダムばら撒き弾
    public static void ShootRandomSpread(Vector3 spawnPos, float speed, int count, Action<Vector3, Vector3> fire)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = UnityEngine.Random.insideUnitCircle.normalized;
            fire(spawnPos, dir * speed);
        }
    }

    // 花びらスピン弾（Velocityを時間で回す）
    public static void ShootSpiral(Vector3 spawnPos, float baseAngle, float spinOffset, int count, float speed, Action<Vector3, Vector3, float> fire)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = baseAngle + spinOffset * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.right;
            fire(spawnPos, dir.normalized * speed, spinOffset);
        }
    }

    // 横から来る弾
    public static void ShootSide(Vector3 spawnPos, float speed, bool fromRight, Action<Vector3, Vector3> fire)
    {
        Vector3 dir = fromRight ? Vector3.left : Vector3.right;
        fire(spawnPos, dir * speed);
    }

}
