using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    private Vector3 moveDirection;

    public void Initialize(Vector3 direction)
    {
        moveDirection = direction.normalized;
        transform.rotation = Quaternion.LookRotation(moveDirection);
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mob"))
        {
            KnockbackMob mob = other.GetComponent<KnockbackMob>();
            if (mob != null)
            {
                /*
                Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                Vector3 knockbackVelocity = knockbackDirection * 10f;
                mob.Initialize(knockbackVelocity);
                */

                Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                mob.Initialize(knockbackDirection);
            }

            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Roulette"))
        {
            Destroy(gameObject);
        }
    }
}
