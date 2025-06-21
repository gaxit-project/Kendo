using UnityEngine;
using Cysharp.Threading.Tasks;
using Utility.ScenLoader;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [SerializeField] private bool knockbackOnMobHit = true;

    [Header("a"), SerializeField] private GameObject pauseUI;
    
    [SerializeField] private InputActionReference _pauseAction;

    [SerializeField] private Button firstSelect;
    
    private CanvasGroup canvasGroup;
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        _pauseAction.action.performed += OnPause;
        canvasGroup = GameObject.Find("UI").GetComponent<CanvasGroup>();
    }

    void Start()
    {
        
        pauseUI.SetActive(false);
        
        Delay(1);
        SoundBGM.Instance.Play("InGame");
    }

    public bool GetKnockbackOnMobHit()
    {
        return knockbackOnMobHit;
    }

    public void SetKnockbackOnMobHit(bool value)
    {
        knockbackOnMobHit = value;
    }

    async void Delay(float delay)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
    }
    
    
    
    
    private void OnEnable() => _pauseAction.action.Enable();
    private void OnDisable() => _pauseAction.action.Disable();

    private void OnPause(InputAction.CallbackContext context)
    {
        
        if (!SceneLoader.IsConfig) // Configシーンをロードしていないとき
        {
            if (!pauseUI.activeSelf) // Pauseが開かれていないなら
            {
                pauseUI.SetActive(true);
                Time.timeScale = 0f;
            }
            else                    // Pauseが開かれているなら
            {
                pauseUI.SetActive(false);
                Time.timeScale = 1f;
            }
            
        }
        
    }


    public Button GetFirstButton()
    {
        return firstSelect;
    }
    
    
    
    
    #region Buttonメソッド

    public void OnBackButtonClick()
    {
        SoundSE.Instance?.Play("Cancel");
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnConfigButtonClick()
    {
        SoundSE.Instance?.Play("Enter");
        canvasGroup.interactable = false;
        SceneLoader.LoadConfig();
    }
    
    public void OnTitleButtonClick()
    {
        SoundSE.Instance?.Play("Enter");
        SoundBGM.Instance.Stop();
        SceneLoader.LoadTitle();
    }
    

    public void OnExitButtonClick()
    {
        SoundSE.Instance?.Play("Cancel");
        SceneLoader.Exit();
    }
    #endregion
    
}
