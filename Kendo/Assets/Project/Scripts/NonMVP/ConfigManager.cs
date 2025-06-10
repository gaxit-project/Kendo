using UnityEngine;
using UnityEngine.UI;
using Utility.ScenLoader;
using UnityEngine.EventSystems;

public class ConfigManager : MonoBehaviour
{
    [SerializeField] private Button firstSelected;
    private CanvasGroup canvasGroup;
    private EventSystem eventSystem;
    void Start()
    {
        firstSelected.Select();
    }
    
    void Update()
    {
        
    }

    public void OnUnload()
    {
        canvasGroup = GameObject.Find("UI").GetComponent<CanvasGroup>();
        canvasGroup.interactable = true;

        
        if (GameObject.Find("TitleManager") != null)
        {
            GameObject.Find("TitleManager").GetComponent<TitleManager>().GetFirstButton().Select();
        }
        
        if (GameObject.Find("InGameManager") != null)
        {
            GameObject.Find("InGameManager").GetComponent<InGameManager>().GetFirstButton().Select();
        }
        
        
        SceneLoader.UnLoadConfig();
    }
    
    
}
