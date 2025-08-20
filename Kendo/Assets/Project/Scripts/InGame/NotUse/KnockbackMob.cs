using UnityEngine;

public class KnockbackMob : MonoBehaviour
{
    private Vector3 velocity;
    private bool isKnockedBack = false;
    private int wallHitCount = 0;

    [SerializeField] private float friction = 0.97f;       // 摩擦係数（速度減衰率）
    [SerializeField] private float minVelocity = 0.1f;      // 速度がこの値を下回ったら停止
    [SerializeField] private float knockbackPower = 15f;    // 吹っ飛び初速度

    public void Initialize(Vector3 knockbackDirection)
    {
        velocity = knockbackDirection.normalized * knockbackPower;
        isKnockedBack = true;
        wallHitCount = 0;
    }

    private void Update()
    {
        if (!isKnockedBack) return;

        Vector3 nextPos = transform.position + velocity * Time.deltaTime;
        Vector3 direction = velocity.normalized;
        float rayLength = (nextPos - transform.position).magnitude + 1f;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, rayLength))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                Vector3 normal = hit.normal;
                velocity = Vector3.Reflect(velocity, normal);

                IncrementWallHitCount();
            }
            else if (hit.collider.CompareTag("Mob"))
            {
                KnockbackMob otherMob = hit.collider.GetComponent<KnockbackMob>();
                if (otherMob != null && !otherMob.IsKnockedBack())
                {
                    MobManager.Instance.ReleaseMob(hit.collider.gameObject); // 静止中のMobを倒す
                }
            }
            else if (hit.collider.CompareTag("Roulette"))
            {
                StopAndRelease();
            }
        }



        // 摩擦による速度減衰
        velocity *= friction;

        // 一定以下の速度になったら停止
        if (velocity.magnitude < minVelocity)
        {
            StopAndRelease();
        }

        // 移動更新
        transform.position += velocity * Time.deltaTime;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isKnockedBack) return;

        if (other.CompareTag("Wall"))
        {
            Vector3 normal = (transform.position - other.ClosestPoint(transform.position)).normalized;
            velocity = Vector3.Reflect(velocity, normal);

            IncrementWallHitCount();
        }
        else if (other.CompareTag("Mob"))
        {
            KnockbackMob otherMob = other.GetComponent<KnockbackMob>();
            if (otherMob != null && !otherMob.IsKnockedBack())
            {
                MobManager.Instance.ReleaseMob(other.gameObject); // 静止中のMobを倒す
            }
        }
        else if (other.CompareTag("Roulette"))
        {
            StopAndRelease();
        }
    }

    private void IncrementWallHitCount()
    {
        wallHitCount++;
        if (wallHitCount >= 2)
        {
            StopAndRelease();
        }
    }

    private void StopAndRelease()
    {
        isKnockedBack = false;
        velocity = Vector3.zero;
        MobManager.Instance.ReleaseMob(gameObject);
    }

    public bool IsKnockedBack() => isKnockedBack;
}
