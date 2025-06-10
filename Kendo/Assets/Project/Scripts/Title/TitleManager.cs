using UnityEngine;
using Utility.ScenLoader;
public class TitleManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // BGMを再生
        SoundBGM.Instance.Play("Title");

    }

    private void OnDestroy()
    {
        // シーン切り替えでBGMを止める
        //SoundBGM.Instance.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Buttonメソッド

    public void OnStartButtonClick()
    {
        SoundBGM.Instance.Stop();
        SceneLoader.LoadInGame();
    }

    public void OnConfigButtonClic()
    {
        SceneLoader.LoadConfig();
    }
    

    public void OnExitButtonClic()
    {
        SceneLoader.Exit();
    }
    #endregion
}
