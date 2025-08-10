using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("弾の性能")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField, Tooltip("弾の質量")] private float mass = 0.2f;

    // この弾の現在の速度ベクトル（外部から参照可能）
    public Vector3 Velocity { get; private set; }

    /// <summary>
    /// 弾を初期化し、指定された方向へ発射する
    /// </summary>
    /// <param name="direction">発射する方向</param>
    public void Initialize(Vector3 direction)
    {
        Vector3 moveDirection = direction.normalized;
        this.Velocity = moveDirection * speed; // 速度ベクトルを保持
        transform.rotation = Quaternion.LookRotation(moveDirection);
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 保持している速度ベクトルに従って、毎フレーム位置を更新
        transform.position += this.Velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mob") || other.CompareTag("TackleMob"))
        {
            MobController mob = other.GetComponent<MobController>();
            if (mob != null)
            {
                // Mobに、Projectile自身と衝突したことを伝える
                // 自身の「速度」「質量」「位置」を相手に渡す
                mob.HandleCollision(this.Velocity, this.mass, this.transform.position);
            }
            // 衝突後、弾は消滅する
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Roulette"))
        {
            Destroy(gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            // Playerには当たらない（何もしない）
        }
    }
}