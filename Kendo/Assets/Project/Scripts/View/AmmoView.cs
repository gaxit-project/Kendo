using UnityEngine;
using UnityEngine.UI;

public class AmmoView : MonoBehaviour
{
    [SerializeField] private Image[] AmmoIcons;

    private void Update()
    {
        int count = PlayerAttack.Instance?.GetCurrentAmmo() ?? 0;

        for (int i = 0; i < AmmoIcons.Length; i++)
        {
            AmmoIcons[i].enabled = i < count;
        }
    }
}
