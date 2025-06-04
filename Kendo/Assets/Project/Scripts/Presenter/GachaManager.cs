using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }

    [SerializeField] private Image gachaImage;                  //ガチャのImageコンポーネント
    [SerializeField] private Sprite[] rollingSprites;           //各種アイテム画像
    [SerializeField] private float rollInterval = 0.1f;         //ガチャの回転間隔
    [SerializeField] private float totalRollTime = 2.0f;        //ガチャの回転時間
    [SerializeField] private float resultDisplayTime = 2.0f;    //結果表示時間

    private bool isRolling = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        }

        yield return new WaitForSeconds(resultDisplayTime);

        gachaImage.enabled = false;
        isRolling = false;
    }


    //効果発動用メソッド
    
    //ハート追加
    private void health()
    {
        int hp;

        PlayerHP.Instance.RecoverHP();
        hp = PlayerHP.Instance?.GetCurrentHP() ?? 0;
        Debug.Log("HP回復：" + hp);
    }

    //ボム発動
    private void bomb()
    {
        PlayerBom.Instance.GachaBom();
        Debug.Log("ボム発動");
    }

    //スピードアップ
    private void speed()
    {
        player.Instance.ChangeSpeed();
        Debug.Log("スピードアップ");
    }
}
