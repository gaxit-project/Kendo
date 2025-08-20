using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;


public class UInull : MonoBehaviour
{
    [SerializeField] private GameObject defaultSelected;

    private void Update()
    {
        // currentSelected が null か、非アクティブなものを選んでいたら復帰
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
