using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISelector : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }
}
