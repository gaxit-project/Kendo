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
            Destroy(gameObject); // �d������C���X�^���X��j��
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

        // �S�e����
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = activeBullets[i];
            bullet.Position += bullet.Velocity * dt;
            bullet.GameObject.transform.position = bullet.Position;

            // ��ʊO�Ȃǂŏ�������
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
        return pos.x > -30f && pos.x < 30f && pos.z > -30f && pos.z < 30f; // �K�X����
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

        // �ԕْe�p
        public float AngularVelocityDeg; // ��]�e�p���x�i���b�x�j

        // �~�^���e�p
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



    // �e�̃p�^�[�����˃��C�u����

public static class BulletPatterns
{
    // �P���F�v���C���[�Ɍ������Ĕ���
    public static void ShootAt(Vector3 spawnPos, Vector3 targetPos, float speed, Action<Vector3, Vector3> fire)
    {
        Vector3 dir = (targetPos - spawnPos).normalized;
        fire(spawnPos, dir * speed);
    }

    // 3Way�e�F�v���C���[���S�A�}angle�Ő��ɔ���
    public static void Shoot3Way(Vector3 spawnPos, Vector3 targetPos, float speed, float angle, Action<Vector3, Vector3> fire)
    {
        Vector3 baseDir = (targetPos - spawnPos).normalized;
        for (int i = -1; i <= 1; i++)
        {
            Vector3 rotatedDir = Quaternion.Euler(0, angle * i, 0) * baseDir;
            fire(spawnPos, rotatedDir * speed);
        }
    }

    // ��^�e�i�C�Ӗ{���j
    public static void ShootFan(Vector3 spawnPos, float startAngle, float totalAngle, int count, float speed, Action<Vector3, Vector3> fire)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (totalAngle / (count - 1)) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.right;
            fire(spawnPos, dir.normalized * speed);
        }
    }

    // �ǔ��e�FVelocity�𖈃t���[���␳����i�ʓr�ǔ��������K�v�j
    public static void ShootHoming(Vector3 spawnPos, float speed, Transform target, Action<Vector3, Vector3, Transform> fire)
    {
        Vector3 dir = (target.position - spawnPos).normalized;
        fire(spawnPos, dir * speed, target);
    }

    // �����_���΂�T���e
    public static void ShootRandomSpread(Vector3 spawnPos, float speed, int count, Action<Vector3, Vector3> fire)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = UnityEngine.Random.insideUnitCircle.normalized;
            fire(spawnPos, dir * speed);
        }
    }

    // �Ԃт�X�s���e�iVelocity�����Ԃŉ񂷁j
    public static void ShootSpiral(Vector3 spawnPos, float baseAngle, float spinOffset, int count, float speed, Action<Vector3, Vector3, float> fire)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = baseAngle + spinOffset * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.right;
            fire(spawnPos, dir.normalized * speed, spinOffset);
        }
    }

    // �����痈��e
    public static void ShootSide(Vector3 spawnPos, float speed, bool fromRight, Action<Vector3, Vector3> fire)
    {
        Vector3 dir = fromRight ? Vector3.left : Vector3.right;
        fire(spawnPos, dir * speed);
    }

}
