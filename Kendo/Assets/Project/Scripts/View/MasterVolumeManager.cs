using UnityEngine;

public class MasterVolumeManager : MonoBehaviour
{
    public static MasterVolumeManager Instance { get; private set; }

    private const string MASTER_VOLUME_KEY = "MASTER_VOLUME";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        float volume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1.0f); // デフォルト1.0
        SetVolume(volume);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
    }

    public float GetVolume()
    {
        return AudioListener.volume;
    }
}
