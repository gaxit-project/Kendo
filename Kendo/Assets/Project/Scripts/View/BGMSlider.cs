using UnityEngine;
using UnityEngine.UI;

public class BGMSlider : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat("BGM_VOLUME", 0.5f);
        bgmSlider.value = volume;
        SoundBGM.Instance.SetVolume(volume);

        bgmSlider.onValueChanged.AddListener(SoundBGM.Instance.SetVolume);
    }
}
