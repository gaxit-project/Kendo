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
        SceneManager.LoadScene("main1-1");
    }
}
