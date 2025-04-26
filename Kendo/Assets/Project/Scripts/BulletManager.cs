using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 1000;

    private List<Bullet> activeBullets = new List<Bullet>();
    private Queue<Bullet> bulletPool = new Queue<Bullet>();

    void Awake()
    {
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

        // ‘S’eˆ—
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = activeBullets[i];
            bullet.Position += bullet.Velocity * dt;
            bullet.GameObject.transform.position = bullet.Position;

            // ‰æ–ÊŠO‚È‚Ç‚ÅÁ‚·ðŒ
            if (!IsInScreen(bullet.Position))
            {
                ReleaseBullet(bullet);
                activeBullets.RemoveAt(i);
            }
        }
    }

    public void SpawnBullet(Vector2 position, Vector2 velocity)
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

    private bool IsInScreen(Vector2 pos)
    {
        return pos.x > -10f && pos.x < 10f && pos.y > -10f && pos.y < 10f; // “K‹X’²®
    }

    private class Bullet
    {
        public GameObject GameObject { get; private set; }
        public Vector2 Position;
        public Vector2 Velocity;

        public Bullet(GameObject obj)
        {
            GameObject = obj;
        }
    }
}
