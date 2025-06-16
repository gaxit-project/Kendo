using System.Collections;
using UnityEngine;

public class MobController : MonoBehaviour
{
    // ノックバック終了とみなす速度の下限
    private const float KnockbackThreshold = 0.5f;

    // 壁反射の上限回数
    private const int MaxBounceCount = 2;

    // 壁との当たり判定に使うSphereCastの半径
    private const float WallCheckRadius = 1f;

    // 壁との当たり判定の距離
    private const float WallCheckDistance = 1f;

    private bool isKnockback = false;
    private Vector3 knockbackVelocity;
    private int bounceCount = 0;

    float time;

    private GameObject player;
    private Coroutine attackCoroutine;
    //見た目変える用
    //private ColorChanger colorChanger;
    private MaterialChanger materialChanger;

    private void Awake()
    {
        time = 0f;
        player = GameObject.FindGameObjectWithTag("Player");
        //colorChanger = GetComponent<ColorChanger>();
        materialChanger = GetComponent<MaterialChanger>();
    }

    private void Update()
    {
        // プレイヤーを常に向く
        if (!isKnockback && player != null && !PlayerBom.bom)
        {
            Vector3 lookPos = player.transform.position;
            lookPos.y = transform.position.y; 
            transform.LookAt(lookPos);
        }

        // 攻撃
        if (!isKnockback && !PlayerBom.bom)
        {
            time += Time.deltaTime;
            if (time >= 3f && player != null)
            {
                Vector3 dir = (player.transform.position - transform.position).normalized;
                if (attackCoroutine != null)
                {
                    StopCoroutine(attackCoroutine);
                }
                StartCoroutine(FireStraightWithDelay(dir));
                time = 0f;
            }
        }

        if (isKnockback)
        {
            // 移動
            transform.position += knockbackVelocity * Time.deltaTime;

            // 壁に当たったら反射
            if (IsHitWall(out Vector3 wallNormal))
            {
                knockbackVelocity = Vector3.Reflect(knockbackVelocity, wallNormal);
                bounceCount++;

                if (bounceCount >= MaxBounceCount)
                {
                    MobManager.Instance.ReleaseMob(gameObject);
                    return;
                }
            }
            else
            {
                // 減速処理（使用しない）: knockbackVelocity *= 0.95f;
            }

            // 一定以下の速度で停止
            if (knockbackVelocity.magnitude < KnockbackThreshold)
            {
                StopKnockback();
            }
        }
    }
    //3連弾
    private IEnumerator FireStraightWithDelay(Vector3 direction)
    {
        for (int i = 0; i < 3; i++)
        {
            BulletManager.Instance.SpawnBullet(transform.position, transform.forward.normalized * 8f);
            SoundSE.Instance?.Play("Shot");
            yield return new WaitForSeconds(0.4f); // 各弾の間隔
            if (isKnockback)
            {
                attackCoroutine = null;
                yield break;
            }
        }
        attackCoroutine = null;
    }



    /// <summary>
    /// ノックバックを指定の速度で開始
    /// </summary>
    public void ApplyKnockback(Vector3 velocity)
    {
        isKnockback = true;
        knockbackVelocity = velocity;
        bounceCount = 0;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        //ここに被弾SE
        SoundSE.Instance?.Play("Hit");


        //colorChanger?.SetColor(Color.red); // ノックバック中に赤
        materialChanger?.SetKnockbackMaterial(); // マテリアル変更

    }

    public bool GetIsKnockback() => isKnockback;
    public Vector3 GetKnockbackVelocity() => knockbackVelocity;
    public int GetBounceCount() => bounceCount;

    private void StopKnockback()
    {
        isKnockback = false;
        knockbackVelocity = Vector3.zero;
        bounceCount = 0;

        //colorChanger?.ResetColor(); // 色を戻す
        materialChanger?.ResetToNormalMaterial(); // 元に戻す

    }

    /// <summary>
    /// 壁との接触判定（SphereCast）
    /// </summary>
    private bool IsHitWall(out Vector3 wallNormal)
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, WallCheckRadius, knockbackVelocity.normalized, out hit, WallCheckDistance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                wallNormal = hit.normal;
                //ここにノックバックSE
                SoundSE.Instance?.Play("Elect");
                // 子のColliderから親にあるBreakableObstacleを探して呼び出す
                BreakableObstacle obstacle = hit.collider.GetComponent<BreakableObstacle>();
                if (obstacle == null)
                {
                    obstacle = hit.collider.GetComponentInParent<BreakableObstacle>();
                }

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



    /// <summary>
    /// 他のMobまたはPlayerに当たったときの処理
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // ノックバックでない状態で Wall に触れたら即死（スコア加算なし）
        if (!isKnockback && other.CompareTag("Wall"))
        {
            MobManager.Instance.ReleaseMobWithoutScore(gameObject);
            return;
        }

        if (!isKnockback) return;

        if (other.CompareTag("Mob"))
        {
            if (InGameManager.Instance.GetKnockbackOnMobHit())
            {
                var otherMob = other.GetComponent<MobController>();
                if (otherMob != null && !otherMob.GetIsKnockback())
                {
                    Vector3 dir = (other.transform.position - transform.position).normalized;
                    otherMob.ApplyKnockback(dir * knockbackVelocity.magnitude);
                }
            }
            else
            {
                MobManager.Instance.ReleaseMob(other.gameObject);
            }
        }
        else if (other.CompareTag("Wall"))
        {
            // 壊れる壁だったら処理する
            var obstacle = other.GetComponent<BreakableObstacle>();
            if (obstacle != null)
            {
                obstacle.Hit(); // 耐久減らす
            }
        }

        if (other.CompareTag("Player"))
        {
            // プレイヤーにはノックバックしない
        }
    }
}
