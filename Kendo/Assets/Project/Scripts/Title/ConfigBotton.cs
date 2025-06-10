using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfigBotton : MonoBehaviour
{
    [SerializeField] private GameObject configCanvas; // 表示させたい設定Canvas

    public void OnClickOpenConfig()
    {
        SceneManager.LoadScene("Config", LoadSceneMode.Additive);
        //configCanvas.SetActive(true); // 表示にする
    }
    public void OnClickCloseConfig()
    {
        configCanvas.SetActive(false); // 非表示にする
    }

}
