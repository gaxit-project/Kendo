using UnityEngine;
using System.Collections.Generic;

public class SoundSE : MonoBehaviour
{
    public static SoundSE Instance { get; private set; }

    [SerializeField] private AudioSource seSource;

    [System.Serializable]
    public class SEEntry
    {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] private List<SEEntry> seList = new List<SEEntry>();
    private Dictionary<string, AudioClip> seDict;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);


        seDict = new Dictionary<string, AudioClip>();
        foreach (var entry in seList)
        {
            if (!seDict.ContainsKey(entry.name))
            {
                seDict.Add(entry.name, entry.clip);
            }
        }

        float volume = PlayerPrefs.GetFloat("SE_VOLUME", 0.5f);
        SetVolume(volume);
    }

    public void Play(string name)
    {
        if (seDict.ContainsKey(name))
        {
            seSource.PlayOneShot(seDict[name]);
        }
        else
        {
            Debug.LogWarning($"SE '{name}' not found.");
        }
    }

    public void SetVolume(float volume)
    {
        seSource.volume = volume;
        PlayerPrefs.SetFloat("SE_VOLUME", volume);
    }

    public float GetVolume() => seSource.volume;
}
