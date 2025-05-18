using UnityEngine;
using UnityEngine.InputSystem.HID;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float power = 15f;
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
            MobController mob = other.GetComponent<MobController>();
            if (mob != null)
            {
                /*
                Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                Vector3 knockbackVelocity = knockbackDirection * 10f;
                mob.Initialize(knockbackVelocity);
                */

                Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                mob.ApplyKnockback(knockbackDirection * power);
            }

            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Roulette"))
        {
            Destroy(gameObject);
        }
        else if (other.CompareTag("Player"))
        {

        }
    }
}
