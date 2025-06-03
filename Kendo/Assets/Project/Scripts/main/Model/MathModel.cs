using UnityEngine;

// クラス名を MathUtility や CustomMath など、MonoBehaviourを継承しないユーティリティクラス名にするのが一般的です。
// もしこのクラスがシーンに配置するものでないなら、MonoBehaviourの継承は不要です。
// 今回は提示されたクラス名をそのまま使いますが、設計に応じて変更を検討してください。
public class MathModel : MonoBehaviour // MonoBehaviourである必要がなければ外してください
{
    /// <summary>
    /// Vector3値を目標値に向かって滑らかに減衰させます。
    /// UnityのVector3.SmoothDampの動作を模倣しようと試みたものです。
    /// </summary>
    /// <param name="current">現在の値（例：現在の位置）。</param>
    /// <param name="target">目標の値（例：目標位置）。</param>
    /// <param name="currentVelocity">現在の速度。この値は関数内で更新されます（参照渡し）。</param>
    /// <param name="smoothTime">目標値に到達するまでのおおよその時間（秒）。小さいほど速く到達します。</param>
    /// <param name="maxSpeed">許容される最大速度。これにより、急激な変化を防ぎます。</param>
    /// <param name="deltaTime">最後のフレームからの経過時間（秒）。</param>
    /// <returns>計算された新しい位置。</returns>
    public static Vector3 SmoothDampCustom(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
    {
        // フレーム間の経過時間が非常に小さい場合は、現在の位置をそのまま返す（変化なし）。
        if (deltaTime <= Mathf.Epsilon)
        {
            return current;
        }

        // smoothTimeが0に近いと計算が不安定になるため、非常に小さい正の値に制限する。
        smoothTime = Mathf.Max(0.0001f, smoothTime);

        // 応答の速さを決める係数omegaを計算 (smoothTimeが短いほどomegaは大きくなる)。
        // これは、目標への収束の速さを示し、バネシステムの角周波数に似た役割を持つ。
        float omega = 2f / smoothTime;
        // omegaとdeltaTimeの積。減衰計算で使う無次元の時間ステップのようなもの。
        float x = omega * deltaTime;
        // 指数関数的な減衰効果 e^(-x) を高速に近似計算するための多項式。
        // これにより、動きが時間と共に滑らかに収束する。
        float expTerm = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

        // 現在位置から目標位置へ向かう差分ベクトル（修正すべき誤差）。
        Vector3 deltaToTarget = target - current;
        // このフレームで発生するであろう位置と速度の変化を計算するための中間的な「変位」または「補正量」。
        // (現在の速度 + 目標へ向かう力に起因する速度変化) × 時間 で計算される。
        Vector3 temp = (currentVelocity + omega * deltaToTarget) * deltaTime;
        
        // 新しい速度を計算。
        // (現在の速度 - 抵抗/ダンピング項) に減衰係数を掛けて、滑らかに速度を変化させる。
        currentVelocity = (currentVelocity - omega * temp) * expTerm;

        // 計算された速度が最大速度を超えないように制限する。
        if (currentVelocity.magnitude > maxSpeed)
        {
            currentVelocity = currentVelocity.normalized * maxSpeed;
        }
        
        // 新しい速度に基づいて、次の位置を計算 (新しい位置 = 現在の位置 + 速度 × 時間)。
        Vector3 newPosition = current + currentVelocity * deltaTime;

        // --- オーバーシュート（目標を行き過ぎる）補正 ---
        // 現在位置からターゲットへの方向ベクトル。
        Vector3 dirToTarget = target - current;
        // そのベクトルの長さの2乗（比較用）。
        float dirToTargetSqMag = dirToTarget.sqrMagnitude;
        // 新しい位置がターゲットに非常に近いかどうかを判定するための閾値。
        bool veryCloseToTarget = (target - newPosition).sqrMagnitude < (0.0001f * 0.0001f);

        // もし新しい位置がターゲットに非常に近い場合。
        if (veryCloseToTarget)
        {
            newPosition = target;           // 位置をターゲットに完全に一致させる。
            currentVelocity = Vector3.zero; // 速度をゼロにして動きを止める。
        }
        // そうでなく、かつ(A)現在位置がターゲットからまだ離れていて、(B)新しい位置がターゲットを通り過ぎたと判断できる場合。
        // (B)の判断: (現在位置からターゲットへのベクトル) と (新しい位置からターゲットへのベクトル) が逆方向を向いている。
        else if (dirToTargetSqMag > Mathf.Epsilon && Vector3.Dot(dirToTarget, target - newPosition) < 0f)
        {
            newPosition = target;           // 位置をターゲットにスナップさせる（行き過ぎを補正）。
            currentVelocity = Vector3.zero; // 速度をゼロにして振動を防ぐ。
        }
        
        // 計算された新しい位置を返す。
        return newPosition;
    }
}