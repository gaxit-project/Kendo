using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSelectSE : MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        SoundSE.Instance?.Play("Select");
    }
}