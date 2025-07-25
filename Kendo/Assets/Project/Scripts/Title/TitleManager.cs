using UnityEngine;
using Utility.ScenLoader;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private Button firstSelect;
    
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GameObject.Find("UI").GetComponent<CanvasGroup>();
    }
    
    void Start()
    {
        // BGMを再生
        SoundBGM.Instance.Play("Title");

    }
    
    public Button GetFirstButton()
    {
        return firstSelect;
    }


    #region Buttonメソッド

    public void OnStartButtonClick()
    {
        SoundSE.Instance?.Play("Enter");
        SoundBGM.Instance.Stop();
        SceneLoader.LoadInGame();
    }

    public void OnConfigButtonClic()
    {
        SoundSE.Instance?.Play("Enter");
        canvasGroup.interactable = false;
        SceneLoader.LoadConfig();
    }
    

    public void OnExitButtonClic()
    {
        SoundSE.Instance?.Play("Cancel");
        SceneLoader.Exit();
    }
    #endregion
}
