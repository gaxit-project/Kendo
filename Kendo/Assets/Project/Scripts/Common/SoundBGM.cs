using UnityEngine;
using System.Collections.Generic;

public class SoundBGM : MonoBehaviour
{
    public static SoundBGM Instance { get; private set; }

    [SerializeField] private AudioSource bgmSource;

    [System.Serializable]
    public class BGMEntry
    {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] private List<BGMEntry> bgmList = new List<BGMEntry>();
    private Dictionary<string, AudioClip> bgmDict;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);


        bgmDict = new Dictionary<string, AudioClip>();
        foreach (var entry in bgmList)
        {
            if (!bgmDict.ContainsKey(entry.name))
            {
                bgmDict.Add(entry.name, entry.clip);
            }
        }

        float volume = PlayerPrefs.GetFloat("BGM_VOLUME", 0.5f);
        SetVolume(volume);
    }

    public void Play(string name)
    {
        if (bgmDict.ContainsKey(name))
        {
            if (bgmSource.clip == bgmDict[name] && bgmSource.isPlaying) return;

            bgmSource.clip = bgmDict[name];
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM '{name}' not found.");
        }
    }

    public void Stop()
    {
        bgmSource.Stop();
    }

    public void SetVolume(float volume)
    {
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat("BGM_VOLUME", volume);
    }

    public float GetVolume() => bgmSource.volume;
}
