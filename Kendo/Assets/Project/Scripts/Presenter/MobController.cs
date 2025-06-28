using System; // TimeSpanのために追加
using System.Threading; // CancellationTokenのために追加
using UnityEngine;
using Cysharp.Threading.Tasks;

#region Attack State Pattern Classes (攻撃ステートパターンのためのクラス群)

/// <summary>
/// 攻撃ステートの振る舞いを定義する抽象基底クラス
/// </summary>
public abstract class AttackState
{
    protected float burstInterval = 0.4f; // 連射間隔

    /// <summary>
    /// 攻撃を実行する非同期メソッド
    /// </summary>
    /// <param name="mob">攻撃を実行するMob本体</param>
    /// <param name="cancellationToken">攻撃を中断するためのトークン</param>
    public abstract UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken);
}

/// <summary>
/// 単発攻撃ステート
/// </summary>
public class SingleShotState : AttackState
{
    public override async UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken)
    {
        // ノックバック中、またはオブジェクトが破棄された場合は即時終了
        if (mob.GetIsKnockback() || cancellationToken.IsCancellationRequested) return;

        mob.PerformShot();
        // UniTaskでは待機処理が不要な場合は書かなくてOK
        await UniTask.CompletedTask;
    }
}

/// <summary>
/// 2連射攻撃ステート
/// </summary>
public class DoubleShotState : AttackState
{
    public override async UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken)
    {
        for (int i = 0; i < 2; i++)
        {
            if (mob.GetIsKnockback() || cancellationToken.IsCancellationRequested) return;
            mob.PerformShot();
            // UniTask.Delayを使用して非同期で待機する
            await UniTask.Delay(TimeSpan.FromSeconds(burstInterval), cancellationToken: cancellationToken);
        }
    }
}

/// <summary>
/// 3連射攻撃ステート
/// </summary>
public class TripleShotState : AttackState
{
    public override async UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken)
    {
        
        if (mob.GetIsKnockback() || cancellationToken.IsCancellationRequested) return;
        mob.FanShot();
        await UniTask.Delay(TimeSpan.FromSeconds(burstInterval), cancellationToken: cancellationToken);
    }
}

/// <summary>
/// 1回限りの特殊攻撃ステート
/// </summary>
public class SpecialAttackState : AttackState
{
    public override async UniTask ExecuteAttack(MobController mob, CancellationToken cancellationToken)
    {
        if (mob.GetIsKnockback() || cancellationToken.IsCancellationRequested) return;
        mob.SuperShot();
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: cancellationToken);
        
        // 実行後、通常の攻撃パターンに戻す
        mob.RevertToNormalAttackState();
    }
}


#endregion


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

    // 自前で管理する速度ベクトル
    private Vector3 currentVelocity;
    private bool isKnockback = false;
    private int bounceCount = 0;
    private float time;

    // 外部コンポーネント/オブジェクトへの参照
    private GameObject player;
    // private Coroutine attackCoroutine; // UniTask化に伴い不要に
    private MaterialChanger materialChanger;
    

    private AttackState _currentAttackState; // 現在の攻撃ステート
    private int _attackPhase = 0; // 現在の攻撃フェーズ（ステートの重複設定を防止）
    private bool _hasPerformedSpecialAttack = false; // 特殊攻撃を一度実行したかのフラグ




    private void Awake()
    {
        time = 0f;
        currentVelocity = Vector3.zero;
        player = GameObject.FindGameObjectWithTag("Player");
        materialChanger = GetComponent<MaterialChanger>();
        

        
        // 初期ステートを設定
        _currentAttackState = new SingleShotState();
        _attackPhase = 1;


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
            
            // 経過時間に応じて攻撃ステートを更新
            UpdateAttackStateBasedOnTime();
            


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
                if (time >= attackSpan && player != null)
                {
                    
                    // コルーチンではなく、現在のステートの攻撃メソッドをUniTaskで実行
                    // .Forget()で実行を待ち合わせず、次のフレームに進む（撃ちっぱなし）
                    _currentAttackState.ExecuteAttack(this, this.GetCancellationTokenOnDestroy()).Forget();
                    
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
    
    
    /// <summary>
    /// 経過時間に基づいて攻撃ステートを更新する
    /// </summary>
    private void UpdateAttackStateBasedOnTime()
    {
        float timer = ScoreManager.Instance.GetMinutes()*60 + ScoreManager.Instance.GetSeconds();
        
        // 5分経過 かつ 特殊攻撃をまだ実行していない場合
        /*
        if (timer > 20f && !_hasPerformedSpecialAttack)
        {
            _currentAttackState = new SpecialAttackState();
            _hasPerformedSpecialAttack = true; // 一度実行したらフラグを立てる
        }
        */
        // 特殊攻撃状態から通常状態に戻った後は、このロジックはスキップ
        if (_currentAttackState is SpecialAttackState) return;

        // 3分経過
        if (timer > 180f && _attackPhase < 3)
        {
            _currentAttackState = new TripleShotState();
            _attackPhase = 3;
        }
        // 1分経過
        else if (timer > 60f && _attackPhase < 2)
        {
            _currentAttackState = new DoubleShotState();
            _attackPhase = 2;
        }
    }

    /// <summary>
    /// 弾を発射する処理（各ステートから呼ばれるヘルパーメソッド）
    /// </summary>
    public void PerformShot()
    {
        BulletManager.Instance.SpawnBullet(transform.position, transform.forward.normalized * 8f);
        SoundSE.Instance?.Play("Shot");
    }
    
    public void FanShot()
    {
        
        int fanBulletCount = 5;
        float fanTotalAngle = 90f;
        float fanBulletSpeed = 8f;
        
        // Mobの正面が、基準となるVector3.rightから何度ズレているかを計算
        float centerAngle = Vector3.SignedAngle(Vector3.right, transform.forward, Vector3.up);

        // ShootFanメソッドに渡す「開始角度」を計算
        float startAngle = centerAngle - (fanTotalAngle / 2f);

        // BulletPatterns.ShootFan に計算した引数を渡して呼び出す
        BulletPatterns.ShootFan(
            transform.position,      // 第1引数: Vector3 spawnPos
            startAngle,         // 第2引数: float startAngle
            fanTotalAngle,      // 第3引数: float totalAngle
            fanBulletCount,     // 第4引数: int count
            fanBulletSpeed      // 第5引数: float speed
        );
            
        SoundSE.Instance?.Play("Shot");
    }
    
    public void SuperShot()
    {
        BulletPatterns.ShootRandomSpread(transform.position, 8f,20);
        
        BulletManager.Instance.SpawnBullet(transform.position, transform.forward.normalized * 8f);
        SoundSE.Instance?.Play("Shot");
    }

    /// <summary>
    /// 特殊攻撃などが終わった後、現在の時間に応じた通常の攻撃状態に戻す
    /// </summary>
    public void RevertToNormalAttackState()
    {
        // _attackPhaseを0にリセットして、UpdateAttackStateBasedOnTimeに判定をやり直させる
        _attackPhase = 0; 
        UpdateAttackStateBasedOnTime();
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
        
        // attackCoroutineの管理は不要になったため削除
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

    // FireStraightWithDelay()コルーチンは不要になったため削除
    
    // 外部から状態を取得するためのメソッド
    public bool GetIsKnockback() => isKnockback;
    public Vector3 GetKnockbackVelocity() => currentVelocity;
    public int GetBounceCount() => bounceCount;
}