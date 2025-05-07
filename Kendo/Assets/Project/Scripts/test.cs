using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject p;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float knockbackSpeed = 5f;

    public bool hit = false;
    private float knockbackTimer = 0f;
    private Vector3 knockbackDirection;

    float time;

    void Start()
    {
        p = GameObject.Find("Player");
        time = 0f;
        hit = false;
    }

    void Update()
    {
        //  プレイヤーボム発動中なら完全停止
        //if (PlayerBom.bom) return;

        if (p != null)
        {
            if (!hit)
            {
                Vector3 direction = (p.transform.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
            }
            else
            {
                // 吹っ飛び中
                transform.position += knockbackDirection * knockbackSpeed * Time.deltaTime;
                knockbackTimer += Time.deltaTime;

                if (knockbackTimer >= 2f)
                {
                    hit = false;
                    knockbackTimer = 0f;
                }
            }
        }

        if (!PlayerBom.bom)
        {
            // 弾発射処理（ボム中はUpdateが止まるため、実行されない）
            if (Input.GetKey(KeyCode.UpArrow))
            {
                BulletPatterns.ShootFan(transform.position, 0f, 30f, 5, 3f, BulletManager.Instance.SpawnBullet);
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                BulletPatterns.ShootSpiral(transform.position, 0f, 30f, 24, 3f, (pos, vel, spin) => BulletManager.Instance.SpawnBullet(pos, vel));
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                for (float i = 15; i > -15; i--)
                {
                    i--;
                    BulletPatterns.ShootSide(new Vector3(29f, 0f, i), 3f, true, BulletManager.Instance.SpawnBullet);
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

            time += Time.deltaTime;
            if (time > 5f)
            {
                BulletPatterns.ShootAt(transform.position, p.transform.position, 10f, BulletManager.Instance.SpawnBullet);
                time = 0f;
            }
        }

        
    }

    // 外部から呼ばれるノックバック処理
    public void Knockback(Vector3 attackerForward)
    {
        hit = true;
        knockbackTimer = 0f;
        knockbackDirection = attackerForward.normalized;
    }

    private void OnDisable()
    {
        // MobManagerに戻す処理など
    }
}
