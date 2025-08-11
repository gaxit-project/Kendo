using System.Collections;
using Main.Presenter;
using UnityEngine;
using UnityEngine.UI;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }

    [Header("Gacha")]
    [SerializeField] private Image gachaImage;                  //ガチャのImage
    [SerializeField] private Sprite[] rollingSprites;           //各種アイテム画像
    [SerializeField] private float rollInterval = 0.1f;         //ガチャの回転間隔
    [SerializeField] private float totalRollTime = 2.0f;        //ガチャの回転時間
    [SerializeField] private float resultDisplayTime = 2.0f;    //結果表示時間
    private bool isRolling = false;

    [Header("Triple7")]
    [SerializeField] private int max7Num = 3;                   // 7を揃える数．3以外は想定してません．
    [SerializeField] private GameObject[] sevenImages;          // 7のオブジェクト
    [SerializeField] private GameObject circlemanager;          // CircleManagerオブジェクト
    [SerializeField] private GameObject destroyEffectPrefab;    // エフェクトのプレハブ
    [SerializeField] private float WallDeactiveTime = 60f;      // 障壁の消去時間
    private int Triple7Cnt = 0;
    private bool isWallDeactive = false;

    //無敵時間用
    private Image[] img = new Image[3];
    [SerializeField] private float switchInterval = 0.3f;
    [SerializeField] private Sprite[] rainbowSprites;
    [SerializeField] private float InvincibleTime = 30f;
    private int rainbowIndex = 0;
    public bool isInvincible = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 7の画像非アクティブ
        foreach (GameObject obj in sevenImages)
        {
            obj.SetActive(false);
        }

        // ガチャImage非アクティブ
        gachaImage.gameObject.SetActive(false);
    }

    //Mobがブラックホールに吸い込まれたら呼び出す
    public void Gacha()
    {
        if (!isRolling)
        {
            StartCoroutine(GachaStart());
            gachaImage.gameObject.SetActive(true);
            Debug.Log("ガチャスタート！");
            SoundSE.Instance?.Play("Slot");
        }
        else
        {

        }
    }

    IEnumerator GachaStart()
    {
        isRolling = true;
        gachaImage.enabled = true;
        float timer = 0f;
        int index = 0;

        while (timer < totalRollTime)
        {
            gachaImage.sprite = rollingSprites[index % rollingSprites.Length];
            index++;
            timer += rollInterval;
            yield return new WaitForSeconds(rollInterval);
        }

        // ランダムなアイテムを表示
        // トリプル7の無敵中はトリプル7がでないようにする
        index = Random.Range(0, rollingSprites.Length);
        index = (Random.Range(0, 2) == 0) ? 0 : 4;
        while (isInvincible && index == 4)
        {
            index = Random.Range(0, rollingSprites.Length);
        }
        Sprite selected = rollingSprites[index];
        gachaImage.sprite = selected;
        SoundSE.Instance?.Play("SlotResult");

        //効果発動
        switch (index)
        {
            case 0:
                health();
                break;
            case 1:
                bomb();
                break;
            case 2:
                speed();
                break;
            case 3:
                ExtendMap();
                break;
            case 4:
                triple7();
                break;
        }

        yield return new WaitForSeconds(resultDisplayTime);

        gachaImage.enabled = false;
        isRolling = false;
    }

    
    // 効果発動用メソッド
    
    // ハート追加
    private void health()
    {
        int hp;

        PlayerHP.Instance.RecoverHP();
        hp = PlayerHP.Instance?.GetCurrentHP() ?? 0;
        Debug.Log("HP回復：" + hp);
    }

    // ボム発動
    private void bomb()
    {
        PlayerBom.Instance.GachaBom();
        Debug.Log("ボム発動");
    }

    // スピードアップ
    private void speed()
    {
        player.Instance.ChangeSpeed();
        Debug.Log("スピードアップ");
    }

    /// <summary>
    /// Mapを拡大します
    /// </summary>
    private void ExtendMap()
    {
        MapPresenter.Instance.ExpandMap();
        Debug.Log("Map拡大");
    }

    // トリプル7
    private void triple7()
    {
        if (sevenImages[Triple7Cnt] != null)
        {
            sevenImages[Triple7Cnt].SetActive(true);
        }

        Triple7Cnt++;

        // 3つ揃ったら無敵
        if (Triple7Cnt == max7Num)
        {
            rainbowIndex = 0;
            StartCoroutine(Invincible());
        }
        Debug.Log("トリプル7：" + Triple7Cnt + "回目");
    }

    IEnumerator Invincible()
    {
        isInvincible = true;
        player.Instance.SetInvincible(true);

        for (int i = 0; i < 3; i++)
        {
            img[i] = sevenImages[i].GetComponent<Image>();
        }

        float time = 0f;
        // レインボー777テクスチャに変更
        while (time < InvincibleTime)
        {
            for (int i = 0;i < 3; i++)
            {
                rainbowIndex = (rainbowIndex + 1) % rainbowSprites.Length;
                img[i].sprite = rainbowSprites[rainbowIndex];
            }
            
            yield return new WaitForSeconds(switchInterval);
            time += switchInterval;
        }

        //777の初期化 & 非アクティブ化
        for (int i = 0; i < 3; i++)
        {
            img[i].sprite = rainbowSprites[0];
        }
        foreach (GameObject obj in sevenImages)
        {
            obj.SetActive(false);
        }

        Triple7Cnt = 0;
        player.Instance.SetInvincible(false);
        isInvincible = false;
    }


    IEnumerator WallBreak()
    {
        BreakableObstacle[] walls = FindObjectsByType<BreakableObstacle>(FindObjectsSortMode.None);

        foreach (BreakableObstacle wall in walls)
        {
            SoundSE.Instance?.Play("Explosion");

            if (destroyEffectPrefab != null)
            {
                Transform child = wall.transform.Find("Cube");
                Vector3 effectPos = child != null ? child.position : wall.transform.position;
                Quaternion effectRot = Quaternion.Euler(90f, 0f, 0f);
                GameObject effect = Instantiate(destroyEffectPrefab, effectPos, effectRot);
                effect.transform.localScale *= 2f;
                Destroy(effect, 1f);
            }

            wall.ForceBreak(); // ← ここを wall.gameObject.SetActive(false) の代わりに
        }

        circlemanager.SetActive(false);

        yield return new WaitForSeconds(WallDeactiveTime);

        circlemanager.SetActive(true);
        circlemanager.GetComponent<CircleManager>().SpawnObstacles();

        foreach (GameObject image in sevenImages)
        {
            image.SetActive(false);
        }
        Triple7Cnt = 0;

        isWallDeactive = false;
    }

}
