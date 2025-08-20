using UnityEngine;
using UnityEngine.UI;

public class MasterSlider : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat("MASTER_VOLUME", 1.0f);
        masterSlider.value = volume;
        MasterVolumeManager.Instance.SetVolume(volume);

        masterSlider.onValueChanged.AddListener(MasterVolumeManager.Instance.SetVolume);
    }
}
