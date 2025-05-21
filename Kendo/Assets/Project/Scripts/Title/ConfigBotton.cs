using UnityEngine;

public class ConfigBotton : MonoBehaviour
{
    [SerializeField] private GameObject configCanvas; // ï\é¶Ç≥ÇπÇΩÇ¢ê›íËCanvas

    public void OnClickOpenConfig()
    {
        configCanvas.SetActive(true); // ï\é¶Ç…Ç∑ÇÈ
    }
    public void OnClickCloseConfig()
    {
        configCanvas.SetActive(false); // îÒï\é¶Ç…Ç∑ÇÈ
    }

}
