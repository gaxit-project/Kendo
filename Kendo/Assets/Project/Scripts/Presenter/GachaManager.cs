using System.Collections;
using Main.Presenter;
using UnityEngine;
using UnityEngine.UI;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }

    [Header("Gacha")]
    [SerializeField] private Image gachaImage;                  //ガチャのImageコンポーネント
    [SerializeField] private Sprite[] rollingSprites;           //各種アイテム画像
    [SerializeField] private float rollInterval = 0.1f;         //ガチャの回転間隔
    [SerializeField] private float totalRollTime = 2.0f;        //ガチャの回転時間
    [SerializeField] private float resultDisplayTime = 2.0f;    //結果表示時間
    private bool isRolling = false;

    [Header("Triple7")]
    [SerializeField] private int max7Num = 3;           // 7を揃える数．3以外は想定してません．
    [SerializeField] private GameObject[] sevenImages;  // 7のオブジェクト
    [SerializeField] private GameObject circlemanager;  // CircleManagerオブジェクト

    private int Triple7Cnt = 0;

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
    }

    //Mobがブラックホールに吸い込まれたら呼び出す
    public void Gacha()
    {
        if (!isRolling)
        {
            StartCoroutine(GachaStart());
            Debug.Log("ガチャスタート！");
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
        index = Random.Range(0, rollingSprites.Length);
        Sprite selected = rollingSprites[index];
        gachaImage.sprite = selected;

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
        if (Triple7Cnt >= max7Num)
        {
            return;
        }

        if (sevenImages[Triple7Cnt] != null)
        {
            sevenImages[Triple7Cnt].SetActive(true);
        }

        Triple7Cnt++;

        // 3つ揃ったら壁消滅
        if (Triple7Cnt == max7Num)
        {
            BreakableObstacle[] walls = FindObjectsByType<BreakableObstacle>(FindObjectsSortMode.None);

            foreach (BreakableObstacle wall in walls)
            {
                wall.gameObject.SetActive(false);
                circlemanager.SetActive(false);
            }
        }
    }
}
