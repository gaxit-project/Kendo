using UnityEngine;

public class KnockbackMob : MonoBehaviour
{
    private Vector3 velocity;
    private bool isKnockedBack = false;
    private int wallHitCount = 0;

    [SerializeField] private float friction = 0.97f;       // ���C�W���i���x�������j
    [SerializeField] private float minVelocity = 0.1f;      // ���x�����̒l������������~
    [SerializeField] private float knockbackPower = 15f;    // ������я����x

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
                    MobManager.Instance.ReleaseMob(hit.collider.gameObject); // �Î~����Mob��|��
                }
            }
            else if (hit.collider.CompareTag("Roulette"))
            {
                StopAndRelease();
            }
        }



        // ���C�ɂ�鑬�x����
        velocity *= friction;

        // ���ȉ��̑��x�ɂȂ������~
        if (velocity.magnitude < minVelocity)
        {
            StopAndRelease();
        }

        // �ړ��X�V
        transform.position += velocity * Time.deltaTime;
    }

    /*
    private void FixedUpdate()
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
        }
    }
    */

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
                MobManager.Instance.ReleaseMob(other.gameObject); // �Î~����Mob��|��
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
