using System.Collections;
using UnityEngine;

public class MobController : MonoBehaviour
{
    [Header("物理パラメータ")]
    [SerializeField, Tooltip("Mobの質量"), Header("質量")]
    private float mass = 1.0f;
    [SerializeField, Tooltip("反発係数 (0に近いほど弾まず、1に近いほどよく跳ね返る)"), Header("反発係数")]
    [Range(0f, 1f)]
    private float restitution = 0.9f;
    [SerializeField, Tooltip("摩擦/空気抵抗係数"), Header("摩擦/空気抵抗係数")]
    [Range(0f, 10f)]
    private float drag = 0.8f;

    [Header("ノックバック設定")]
    [SerializeField, Tooltip("ノックバック終了とみなす速度の下限")]
    private float stopThreshold = 0.1f;
    [SerializeField, Tooltip("壁での最大反射回数")]
    private int maxBounceCount = 2;
    [SerializeField, Tooltip("壁との当たり判定に使うSphereCastの半径")]
    private float wallCheckRadius = 1f;
    [SerializeField, Tooltip("壁との当たり判定の距離")]
    private float wallCheckDistance = 0.5f;

    // 自前で管理する速度ベクトル
    private Vector3 currentVelocity;
    private bool isKnockback = false;
    private int bounceCount = 0;
    private float time;

    // 外部コンポーネント/オブジェクトへの参照
    private GameObject player;
    private Coroutine attackCoroutine;
    private MaterialChanger materialChanger;

    private void Awake()
    {
        time = 0f;
        currentVelocity = Vector3.zero;
        player = GameObject.FindGameObjectWithTag("Player");
        materialChanger = GetComponent<MaterialChanger>();
    }

    private void Update()
    {
        // ノックバック中で、速度が非常に小さくなったら停止させる
        if (isKnockback && currentVelocity.magnitude < stopThreshold)
        {
            StopKnockback();
        }

        // ノックバック中でなければ通常の行動（向き変更や攻撃）
        if (!isKnockback)
        {
            // プレイヤーを常に向く
            if (player != null && !PlayerBom.bom)
            {
                Vector3 lookPos = player.transform.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);
            }

            // 攻撃
            if (!PlayerBom.bom)
            {
                time += Time.deltaTime;
                if (time >= 3f && player != null)
                {
                    if (attackCoroutine != null)
                    {
                        StopCoroutine(attackCoroutine);
                    }
                    StartCoroutine(FireStraightWithDelay());
                    time = 0f;
                }
            }
        }
        else // ノックバック中
        {
            // 速度に基づいて位置を更新
            transform.position += currentVelocity * Time.deltaTime;

            // 抵抗による減速処理
            currentVelocity -= currentVelocity * drag * Time.deltaTime;

            // 壁との反射チェック
            if (IsHitWall(out Vector3 wallNormal))
            {
                transform.position += wallNormal * 0.05f; // 壁へのめり込み防止
                currentVelocity = Vector3.Reflect(currentVelocity, wallNormal) * restitution;

                bounceCount++;
                SoundSE.Instance?.Play("Elect");

                // 壊れる壁の処理
                // ※IsHitWall内でRaycastHitが取得できないため、別途判定が必要
                // このサンプルでは省略しますが、IsHitWallがRaycastHitを返すように変更すると実装可能です。

                if (bounceCount >= maxBounceCount)
                {
                    MobManager.Instance.ReleaseMob(gameObject); // プロジェクト固有のMobマネージャー
                    return;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ノックバック中でない状態で壁に触れたら即死
        if (!isKnockback && other.CompareTag("Wall"))
        {
            MobManager.Instance.ReleaseMobWithoutScore(gameObject);
            return;
        }

        // 他のMobとの衝突
        if (other.CompareTag("Mob"))
        {
            // 衝突計算が二重に行われるのを防ぐため、ユニークなIDが小さい方でだけ処理を実行
            if (gameObject.GetInstanceID() < other.gameObject.GetInstanceID())
            {
                var otherMob = other.GetComponent<MobController>();
                if (otherMob != null)
                {
                    HandleCollisionWithOtherMob(otherMob);
                }
            }
        }
    }

    /// <summary>
    /// Projectileと衝突した際の処理 (Projectile.csから呼び出される)
    /// </summary>
    public void HandleCollision(Vector3 projectileVelocity, float projectileMass, Vector3 projectilePosition)
    {
        StartKnockback();
        Vector3 normal = (transform.position - projectilePosition).normalized;

        CalculateReflectionVelocity(
            this.currentVelocity, projectileVelocity, this.mass, projectileMass, this.restitution, normal,
            out Vector3 myNewVelocity, out Vector3 _);

        this.currentVelocity = myNewVelocity;
        SoundSE.Instance?.Play("Hit");
    }

    /// <summary>
    /// 他のMobと衝突した際の処理
    /// </summary>
    private void HandleCollisionWithOtherMob(MobController otherMob)
    {
        // 衝突連鎖を有効にするかどうかのフラグチェック
        if (!InGameManager.Instance.GetKnockbackOnMobHit())
        {
            MobManager.Instance.ReleaseMob(otherMob.gameObject);
            return;
        }

        // 相手が既にノックバック中の場合は計算しない（連鎖衝突の挙動を安定させるため）
        if (otherMob.isKnockback) return;

        StartKnockback();
        otherMob.StartKnockback();

        Vector3 normal = (otherMob.transform.position - this.transform.position).normalized;

        CalculateReflectionVelocity(
            this.currentVelocity, otherMob.currentVelocity, this.mass, otherMob.mass, this.restitution, normal,
            out Vector3 myNewVelocity, out Vector3 otherNewVelocity);

        this.currentVelocity = myNewVelocity;
        otherMob.currentVelocity = otherNewVelocity;

        // オブジェクトのめり込み補正
        float myRadius = GetComponent<Collider>().bounds.extents.x;
        float otherRadius = otherMob.GetComponent<Collider>().bounds.extents.x;
        float distance = Vector3.Distance(transform.position, otherMob.transform.position);
        float overlap = (myRadius + otherRadius) - distance;

        if (overlap > 0)
        {
            Vector3 separationVector = normal * overlap * 0.5f;
            transform.position -= separationVector;
            otherMob.transform.position += separationVector;
        }

        SoundSE.Instance?.Play("Hit");
    }

    /// <summary>
    /// 2つの物体の衝突後の速度を計算する
    /// </summary>
    private void CalculateReflectionVelocity(Vector3 v1, Vector3 v2, float m1, float m2, float e, Vector3 normal, out Vector3 newV1, out Vector3 newV2)
    {
        float v1n_scalar = Vector3.Dot(v1, normal);
        float v2n_scalar = Vector3.Dot(v2, normal);
        Vector3 v1t_vec = v1 - v1n_scalar * normal;
        Vector3 v2t_vec = v2 - v2n_scalar * normal;
        float newV1n_scalar = (m1 * v1n_scalar + m2 * v2n_scalar - m2 * e * (v1n_scalar - v2n_scalar)) / (m1 + m2);
        float newV2n_scalar = (m1 * v1n_scalar + m2 * v2n_scalar + m1 * e * (v1n_scalar - v2n_scalar)) / (m1 + m2);
        newV1 = newV1n_scalar * normal + v1t_vec;
        newV2 = newV2n_scalar * normal + v2t_vec;
    }

    private void StartKnockback()
    {
        if (isKnockback) return;
        isKnockback = true;
        bounceCount = 0;
        materialChanger?.SetKnockbackMaterial();

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private void StopKnockback()
    {
        isKnockback = false;
        currentVelocity = Vector3.zero;
        materialChanger?.ResetToNormalMaterial();
    }

    private bool IsHitWall(out Vector3 wallNormal)
    {
        RaycastHit hit;
        if (currentVelocity.sqrMagnitude < 0.01f)
        {
            wallNormal = Vector3.zero;
            return false;
        }

        if (Physics.SphereCast(transform.position, wallCheckRadius, currentVelocity.normalized, out hit, wallCheckDistance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                wallNormal = hit.normal;

                // 壊れる壁の処理
                BreakableObstacle obstacle = hit.collider.GetComponentInParent<BreakableObstacle>();
                if (obstacle != null)
                {
                    obstacle.Hit();
                }
                return true;
            }
        }
        wallNormal = Vector3.zero;
        return false;
    }

    // 3連弾攻撃コルーチン
    private IEnumerator FireStraightWithDelay()
    {
        for (int i = 0; i < 3; i++)
        {
            // isKnockback状態になったら攻撃を即時中断する
            if (isKnockback)
            {
                attackCoroutine = null;
                yield break;
            }

            BulletManager.Instance.SpawnBullet(transform.position, transform.forward.normalized * 8f);
            SoundSE.Instance?.Play("Shot");
            yield return new WaitForSeconds(0.4f);
        }
        attackCoroutine = null;
    }

    // 外部から状態を取得するためのメソッド
    public bool GetIsKnockback() => isKnockback;
    public Vector3 GetKnockbackVelocity() => currentVelocity;
    public int GetBounceCount() => bounceCount;
}