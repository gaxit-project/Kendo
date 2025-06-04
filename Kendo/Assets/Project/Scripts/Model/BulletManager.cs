using System.Collections.Generic;
using UnityEngine;
using Main.Presenter;

/// <summary>
/// 弾の生成・更新・破棄・オブジェクトプール管理を行うクラス
/// </summary>
public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; private set; }

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int initialPoolSize = 100;
    [SerializeField] private MapPresenter mapPresenter;
    private float mapSize;

    private readonly Queue<Bullet> bulletPool = new Queue<Bullet>();
    private readonly List<Bullet> activeBullets = new List<Bullet>();

    private void Awake()
    {
        Instance = this;

        // プールの初期化
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            bulletPool.Enqueue(new Bullet(obj));
        }
    }
    
    private void Start()
    {
        if (mapPresenter == null)
        {
            mapPresenter = FindObjectOfType<MapPresenter>();
        }
        else
        {
            MapPresenter.OnMapPresenterReady += InitializeWithMapPresenter;
        }

        // 2. MapPresenterのマップサイズ更新イベントを購読
        mapPresenter.OnMapSizeUpdated += HandleMapSizeUpdated;
    }

    private void Update()
    {
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = activeBullets[i];

            if (bullet.IsTrueSpiral)
            {
                // 真のスパイラル軌道：角度回転と半径増加
                bullet.AngleDeg += bullet.AngularSpeedDeg * Time.deltaTime;
                bullet.Radius += bullet.RadiusGrowthPerSec * Time.deltaTime;

                float rad = bullet.AngleDeg * Mathf.Deg2Rad;
                bullet.Position = bullet.Center + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * bullet.Radius;
            }
            else if (bullet.IsCircular)
            {
                // 円運動：角度に基づき円周上を移動
                bullet.AngleDeg += bullet.AngularSpeedDeg * Time.deltaTime;
                float rad = bullet.AngleDeg * Mathf.Deg2Rad;
                bullet.Position = bullet.Center + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * bullet.Radius;
            }
            else
            {
                // 通常の直進弾
                bullet.Position += bullet.Velocity * Time.deltaTime;
            }

            bullet.GameObject.transform.position = bullet.Position;

            /*
            // 一定距離以上離れたら破棄（仮）
            if (bullet.Position.magnitude > 100f)
            {
                bullet.GameObject.SetActive(false);
                bulletPool.Enqueue(bullet);
                activeBullets.RemoveAt(i);
            }
            */
            
            MapOutBullets(bullet, i);
        }

    }

    /// <summary>
    /// 弾を生成して有効化する（円運動、真のスパイラルにも対応）
    /// </summary>
    public void SpawnBullet(
        Vector3 position,
        Vector3 velocity,
        bool isCircular = false,
        Vector3 center = default,
        float radius = 0f,
        float startAngleDeg = 0f,
        float angularSpeedDeg = 0f,
        float radiusGrowthPerSec = 0f,
        bool isTrueSpiral = false)
    {
        if (bulletPool.Count > 0)
        {
            Bullet bullet = bulletPool.Dequeue();
            bullet.Position = position;
            bullet.Velocity = velocity;

            bullet.IsCircular = isCircular;
            bullet.Center = center;
            bullet.Radius = radius;
            bullet.AngleDeg = startAngleDeg;
            bullet.AngularSpeedDeg = angularSpeedDeg;

            bullet.RadiusGrowthPerSec = radiusGrowthPerSec;
            bullet.IsTrueSpiral = isTrueSpiral;

            bullet.GameObject.transform.position = position;
            bullet.GameObject.SetActive(true);
            activeBullets.Add(bullet);
        }
    }

    /// <summary>
    /// 画面上のすべての弾を削除（非アクティブ化）してプールに戻す
    /// </summary>
    public void ClearAllBullets()
    {
        foreach (var bullet in activeBullets)
        {
            bullet.GameObject.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
        activeBullets.Clear();
    }

    /// <summary>
    /// マップ外の弾を削除（非アクティブ化）してプールに戻す
    /// </summary>
    /// <param name="bullet"></param>
    /// <param name="indexInActiveList"></param>
    public void MapOutBullets(Bullet bullet, int indexInActiveList)
    {
        bool isOutOfMap = bullet.Position.x < -mapSize || bullet.Position.x > mapSize ||
                          bullet.Position.z < -mapSize || bullet.Position.z > mapSize;

        if (isOutOfMap)
        {
            Debug.Log("消したよ");
            bullet.GameObject.SetActive(false);
            bulletPool.Enqueue(bullet);
            activeBullets.RemoveAt(indexInActiveList);
        }
    }
    
    private void InitializeWithMapPresenter()
    {

        // OnMapPresenterReady から呼ばれた場合、mapPresenter は MapPresenter.Instance で確実に取得できるか、
        if (mapPresenter == null && MapPresenter.Instance != null) 
        {
            mapPresenter = MapPresenter.Instance;
        }
        
        if (mapPresenter == null) {

            HandleMapSizeUpdated(100f); // 再度フォールバック
            return;
        }

        mapSize = mapPresenter.GetCurrentMapSize();

        // イベントから一度だけ呼び出されるように、購読を解除する
        MapPresenter.OnMapPresenterReady -= InitializeWithMapPresenter;
    }
    
    
    /// <summary>
    /// MapPresenterからマップサイズ変更の通知を受けた際のイベントハンドラ。
    /// </summary>
    private void HandleMapSizeUpdated(float newMapSize)
    {
        mapSize = newMapSize;
    }
}

/// <summary>
/// 弾1発分の情報（挙動・座標など）
/// </summary>
public class Bullet
{
    public GameObject GameObject;

    public Vector3 Position;
    public Vector3 Velocity;

    public bool IsCircular;
    public Vector3 Center;
    public float Radius;
    public float AngleDeg;
    public float AngularSpeedDeg;

    public float RadiusGrowthPerSec; // 真のスパイラル用：半径の増加速度
    public bool IsTrueSpiral; // 真のスパイラル判定フラグ

    public Bullet(GameObject obj)
    {
        GameObject = obj;
    }
}
