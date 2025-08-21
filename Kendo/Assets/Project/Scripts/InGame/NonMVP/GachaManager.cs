using System.Collections;
using Main.Presenter;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    //Feverカットイン
    [SerializeField] private GameObject feverRoot;      // FEVERパネル
    [SerializeField] private TMP_Text feverText;        // テキスト
    [SerializeField] private float feverDuration = 1.2f;
    [SerializeField] private bool pauseDuringFever = true;
    private bool _feverRunning = false;
    [SerializeField] private bool feverRainbow = true;
    [SerializeField] private float feverHueSpeed = 0.9f;       // 色相回転速度
    [SerializeField] private float feverGradientSpread = 0.28f;// 4隅の位相差
    [SerializeField] private float feverSaturation = 1f;
    [SerializeField] private float feverValue = 1f;
    [SerializeField] private float feverTextScale = 3f;
    [SerializeField] private float feverPopStart = 0.6f;
    [SerializeField] private float feverMaxScale = 6f;   // 最終文字倍率
    [SerializeField] private float feverGrowDuration = 3f; // 文字拡大秒数



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
        while (isInvincible && index == 4)
        {
            index = Random.Range(0, rollingSprites.Length);
        }
        Sprite selected = rollingSprites[index];
        gachaImage.sprite = selected;
        //SoundSE.Instance?.Play("SlotResult");

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
        SoundSE.Instance?.Play("Recovery");
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
        SoundSE.Instance?.Play("SpeedUp");
    }

    /// <summary>
    /// Mapを拡大します
    /// </summary>
    private void ExtendMap()
    {
        MapPresenter.Instance.ExpandMap();
        Debug.Log("Map拡大");
        SoundSE.Instance?.Play("Expansion");
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
            //StartCoroutine(Invincible());
            StartCoroutine(FeverThenInvincible());
            SoundSE.Instance?.Play("777");
        }
        else
        {
            SoundSE.Instance?.Play("SlotResult");
        }
        Debug.Log("トリプル7：" + Triple7Cnt + "回目");
    }

    IEnumerator Invincible()
    {
        isInvincible = true;
        player.Instance.SetInvincible(true);

        SoundBGM.Instance.Stop();
        SoundBGM.Instance.Play("777");

        for (int i = 0; i < 3; i++)
        {
            img[i] = sevenImages[i].GetComponent<Image>();
        }

        float time = 0f;
        bool slowBlinkStarted = false;
        bool fastBlinkStarted = false;

        // レインボー777テクスチャに変更
        while (time < InvincibleTime)
        {
            float remaining = InvincibleTime - time;
            // 残り10秒で「ゆっくり点滅」開始
            if (!slowBlinkStarted && remaining <= 10f)
            {
                player.Instance?.StartPreEndBlink(false); // false=ゆっくり
                slowBlinkStarted = true;
            }
            // 残り3秒で「速い点滅」に切り替え
            if (!fastBlinkStarted && remaining <= 3f)
            {
                player.Instance?.SetPreEndBlinkSpeed(true); // true=速い
                fastBlinkStarted = true;
            }

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
            Debug.Log("トリプル7非アクティブ");
        }
        //点滅の停止処理
        player.Instance?.StopPreEndBlink();

        Triple7Cnt = 0;
        player.Instance.SetInvincible(false);
        isInvincible = false;
        //SoundBGM.Instance.Stop();
        SoundBGM.Instance.Play("InGame");
        Debug.Log("無敵状態終わり" + isInvincible);
    }

    //Feverカットイン
    private IEnumerator FeverThenInvincible()
    {
        if (_feverRunning) yield break;
        _feverRunning = true;

        float prevTimeScale = Time.timeScale;
        if (pauseDuringFever) Time.timeScale = 0f;

        // Canvasを最前面に
        if (feverRoot != null)
        {
            feverRoot.SetActive(true);
            var localCanvas = feverRoot.GetComponent<Canvas>();
            if (!localCanvas) localCanvas = feverRoot.AddComponent<Canvas>();
            localCanvas.overrideSorting = true;
            localCanvas.sortingOrder = 999;

            var prt = feverRoot.GetComponent<RectTransform>();
            if (prt != null) { prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one; prt.offsetMin = prt.offsetMax = Vector2.zero; }
        }

        // テキストの設定
        RectTransform rt = null;
        Vector3 baseScale = Vector3.one;
        if (feverText != null)
        {
            feverText.text = "FEVER";
            feverText.alpha = 1f;
            rt = feverText.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            // 最初の倍率を決定
            baseScale = Vector3.one * feverTextScale;
            Vector3 start = baseScale * feverPopStart;
            rt.localScale = start;

            float tIn = 0f, inDur = 0.18f;
            while (tIn < inDur)
            {
                tIn += Time.unscaledDeltaTime;
                rt.localScale = Vector3.Lerp(start, baseScale, tIn / inDur);
                yield return null;
            }

            if (feverRainbow) feverText.enableVertexGradient = true;
        }

        SoundBGM.Instance.Stop();
        SoundSE.Instance?.Play("Fever");

        //文字の演出
        float elapsed = 0f, hueBase = 0f;
        float displayTotal = Mathf.Max(feverDuration, feverGrowDuration); // 表示総時間
        Vector3 maxScale = Vector3.one * feverMaxScale;

        while (elapsed < displayTotal)
        {
            elapsed += Time.unscaledDeltaTime;

            // 虹色に光る
            if (feverRainbow && feverText != null)
            {
                hueBase = Mathf.Repeat(hueBase + feverHueSpeed * Time.unscaledDeltaTime, 1f);
                float h1 = hueBase;
                float h2 = Mathf.Repeat(hueBase + feverGradientSpread, 1f);
                float h3 = Mathf.Repeat(hueBase + feverGradientSpread * 2f, 1f);
                float h4 = Mathf.Repeat(hueBase + feverGradientSpread * 3f, 1f);
                Color c1 = Color.HSVToRGB(h1, feverSaturation, feverValue);
                Color c2 = Color.HSVToRGB(h2, feverSaturation, feverValue);
                Color c3 = Color.HSVToRGB(h3, feverSaturation, feverValue);
                Color c4 = Color.HSVToRGB(h4, feverSaturation, feverValue);
                feverText.colorGradient = new VertexGradient(c1, c2, c3, c4);
            }
            //拡大していく
            if (rt != null)
            {
                float tGrow = Mathf.Clamp01(elapsed / feverGrowDuration);
                float e = 1f - Mathf.Pow(1f - tGrow, 3f);  // EaseOutCubic
                rt.localScale = Vector3.Lerp(baseScale, maxScale, e);
            }

            yield return null;
        }


        // 演出終了処理
        if (feverText != null)
        {
            if (feverRainbow) { feverText.enableVertexGradient = false; feverText.color = Color.white; }
            if (rt != null) rt.localScale = Vector3.one;
        }
        if (feverRoot != null) feverRoot.SetActive(false);
        if (pauseDuringFever) Time.timeScale = prevTimeScale;
        _feverRunning = false;

        StartCoroutine(Invincible());
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
