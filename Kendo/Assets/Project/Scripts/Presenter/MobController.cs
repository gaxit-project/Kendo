using UnityEngine;

public class MobController : MonoBehaviour
{
    private Vector3 velocity;
    private bool isKnockbacked = false;
    private int wallHitCount = 0;
    private const float moveSpeed = 10f;

    private KnockbackMob knockbackMob;

    void Awake()
    {
        knockbackMob = GetComponent<KnockbackMob>();
    }

    public void Knockback(Vector3 hitDirection)
    {
        if (knockbackMob != null)
        {
            knockbackMob.Initialize(hitDirection.normalized * 10f);
        }
    }

    void Update()
    {
        if (!isKnockbacked) return;

        transform.position += velocity * Time.deltaTime;

        CheckCollision();
    }

    private void CheckCollision()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.3f);

        foreach (var hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            if (hit.CompareTag("Wall"))
            {
                wallHitCount++;
                if (wallHitCount >= 2)
                {
                    isKnockbacked = false;
                    MobManager.Instance.ReleaseMob(gameObject);
                }
                else
                {
                    Vector3 normal = (transform.position - hit.ClosestPoint(transform.position)).normalized;
                    velocity = Vector3.Reflect(velocity, normal);
                }
            }
            else if (hit.CompareTag("Mob"))
            {
                MobController other = hit.GetComponent<MobController>();
                if (other != null && !other.isKnockbacked)
                {
                    MobManager.Instance.ReleaseMob(other.gameObject); // 当たった相手を倒す
                }
            }
            else if (hit.CompareTag("Roulette"))
            {
                isKnockbacked = false;
                MobManager.Instance.ReleaseMob(gameObject);
            }
        }
    }
}
