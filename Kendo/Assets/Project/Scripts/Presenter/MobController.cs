using UnityEngine;
using InGame.Model;

public class MobController : MonoBehaviour
{
    [Header("mobパラメータ")]
    [SerializeField, Tooltip("攻撃頻度"), Header("攻撃間隔")]
    private float attackSpan = 4.0f;
    
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

    // モデルのインスタンス
    private PhysicsModel _physicsModel;
    private IEnemyModel _enemyModel;

    // 外部コンポーネント/オブジェクトへの参照
    private MaterialChanger _materialChanger;
    
    // MobManagerから各モデルへアクセスするための公開メソッド
    public PhysicsModel GetPhysicsModel() => _physicsModel;
    public IEnemyModel GetEnemyModel() => _enemyModel;

    private void Awake()
    {
        _physicsModel = new PhysicsModel();
        // インスペクターの値をコンストラクタに渡してEnemyModelを生成
        _enemyModel = new EnemyModel(attackSpan, mass, restitution, drag, stopThreshold, maxBounceCount, wallCheckRadius, wallCheckDistance);
        _materialChanger = GetComponent<MaterialChanger>();
    }

    private void OnEnable()
    {
        // オブジェクトプールで再利用されることを想定し、有効化時にモデルをリセット
        _physicsModel = new PhysicsModel();
        _enemyModel = new EnemyModel(attackSpan, mass, restitution, drag, stopThreshold, maxBounceCount, wallCheckRadius, wallCheckDistance);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // ノックバック中でない状態で壁に触れたら即死
        if (!_physicsModel.GetIsKnockback() && other.CompareTag("Wall"))
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

        _physicsModel.CalculateReflectionVelocity(
            _physicsModel.GetCurrentVelocity(), projectileVelocity, _enemyModel.GetMass(), projectileMass, _enemyModel.GetRestitution(), normal,
            out Vector3 myNewVelocity, out Vector3 _);

        _physicsModel.SetCurrentVelocity(myNewVelocity);
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
        if (otherMob.GetPhysicsModel().GetIsKnockback()) return;

        StartKnockback();
        otherMob.StartKnockback();

        Vector3 normal = (otherMob.transform.position - transform.position).normalized;

        _physicsModel.CalculateReflectionVelocity(
            _physicsModel.GetCurrentVelocity(), otherMob.GetPhysicsModel().GetCurrentVelocity(), _enemyModel.GetMass(), otherMob.GetEnemyModel().GetMass(), _enemyModel.GetRestitution(), normal,
            out Vector3 myNewVelocity, out Vector3 otherNewVelocity);

        _physicsModel.SetCurrentVelocity(myNewVelocity);
        otherMob.GetPhysicsModel().SetCurrentVelocity(otherNewVelocity);

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

    public void StartKnockback()
    {
        if (_physicsModel.GetIsKnockback()) return;
        _physicsModel.SetIsKnockback(true);
        _physicsModel.SetBounceCount(0);
        _materialChanger?.SetKnockbackMaterial();
    }

    public void StopKnockback()
    {
        _physicsModel.SetIsKnockback(false);
        _physicsModel.SetCurrentVelocity(Vector3.zero);
        _materialChanger?.ResetToNormalMaterial();
    }

    public bool IsHitWall(out Vector3 wallNormal)
    {
        RaycastHit hit;
        if (_physicsModel.GetCurrentVelocity().sqrMagnitude < 0.01f)
        {
            wallNormal = Vector3.zero;
            return false;
        }

        if (Physics.SphereCast(transform.position, _enemyModel.GetWallCheckRadius(), _physicsModel.GetCurrentVelocity().normalized, out hit, _enemyModel.GetWallCheckDistance()))
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

    // EnemyModelからの呼び出しに応えるヘルパーメソッド
    public void PerformShot() => _enemyModel.PerformShot(transform);
    public void FanShot() => _enemyModel.FanShot(transform);
    public void SuperShot() => _enemyModel.SuperShot(transform);
    public void RevertToNormalAttackState() => _enemyModel.RevertToNormalAttackState();

    // 外部から状態を取得するためのメソッド
    public bool GetIsKnockback() => _physicsModel.GetIsKnockback();
    public Vector3 GetKnockbackVelocity() => _physicsModel.GetCurrentVelocity();
    public int GetBounceCount() => _physicsModel.GetBounceCount();
}