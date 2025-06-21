using UnityEngine;
using UnityEngine.SceneManagement;

public class StartController : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void onClickStartButton()
    {
        SoundSE.Instance?.Play("Enter");
        //SceneManager.LoadScene("main1-1");
        FadeManager.Instance.FadeOutAndLoad("main1-1");
    }
    public void onClickTitleButton()
    {
        SoundSE.Instance?.Play("Enter");
        //SceneManager.LoadScene("Title");
        FadeManager.Instance.FadeOutAndLoad("Title");
    }
}
