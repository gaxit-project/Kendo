using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;


public class UInull : MonoBehaviour
{
    [SerializeField] private GameObject defaultSelected;

    private void Update()
    {
        // currentSelected �� null ���A��A�N�e�B�u�Ȃ��̂�I��ł����畜�A
        if (EventSystem.current.currentSelectedGameObject == null ||
            !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
        {
            if (defaultSelected != null && defaultSelected.activeInHierarchy)
            {
                StartCoroutine(SelectNextFrame(defaultSelected));
            }
        }
    }

    private IEnumerator SelectNextFrame(GameObject button)
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(button);
    }
}
