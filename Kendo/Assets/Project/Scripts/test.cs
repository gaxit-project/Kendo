using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject p;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            //BulletManager.Instance.SpawnBullet(transform.position, p.transform.position);
            //BulletPatterns.ShootSpiral(transform.position, 0f, 30f, 24, 3f, (pos, vel, spin) => BulletManager.Instance.SpawnBullet(pos, vel));
            BulletPatterns.ShootFan(transform.position, 0f, 30f, 5, 3f, BulletManager.Instance.SpawnBullet);
            //BulletPatterns.ShootRandomSpread(transform.position, 3f, 5, BulletManager.Instance.SpawnBullet);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            //BulletManager.Instance.SpawnBullet(transform.position, p.transform.position);
            BulletPatterns.ShootSpiral(transform.position, 0f, 30f, 24, 3f, (pos, vel, spin) => BulletManager.Instance.SpawnBullet(pos, vel));
            //BulletPatterns.ShootFan(transform.position, 0f, 30f, 5, 3f, BulletManager.Instance.SpawnBullet);
            //BulletPatterns.ShootRandomSpread(transform.position, 3f, 5, BulletManager.Instance.SpawnBullet);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            for (float i = 15; i > -15; i--)
            {

                BulletPatterns.ShootSide(new Vector3(29f, 0f, i), 3f, true, BulletManager.Instance.SpawnBullet);
                i--;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            for (float i = 15; i > -15; i--)
            {
                i--;
                BulletPatterns.ShootSide(new Vector3(-29f, 0f, i), 3f, false, BulletManager.Instance.SpawnBullet);
            }
        }
    }
}
