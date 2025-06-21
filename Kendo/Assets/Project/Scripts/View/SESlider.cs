using UnityEngine;
using UnityEngine.UI;

public class SESlider : MonoBehaviour
{
    [SerializeField] private Slider seSlider;

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat("SE_VOLUME", 0.5f);
        seSlider.value = volume;
        SoundSE.Instance.SetVolume(volume);

        seSlider.onValueChanged.AddListener(SoundSE.Instance.SetVolume);
    }
}