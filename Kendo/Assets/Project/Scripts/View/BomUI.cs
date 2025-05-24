using UnityEngine;
using UnityEngine.UI;

public class BomUI : MonoBehaviour
{
    [SerializeField] private Image[] bomIcons; // 3‚Â‚ÌƒAƒCƒRƒ“‚ð“o˜^‚µ‚Ä‚¨‚­

    private void Update()
    {
        int count = PlayerBom.Instance?.GetBomCount() ?? 0;

        for (int i = 0; i < bomIcons.Length; i++)
        {
            bomIcons[i].enabled = i < count;
        }
    }
}
