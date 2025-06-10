using UnityEngine;
using UnityEngine.UI;
using Utility.ScenLoader;

public class ConfigManager : MonoBehaviour
{
    [SerializeField] private Button firstSelected;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        firstSelected.Select();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnUnload()
    {
        SceneLoader.UnLoadConfig();
    }
    
    
}
