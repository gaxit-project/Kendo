using UnityEngine;
using UnityEngine.UI;

public class Gacha : MonoBehaviour
{
    [SerializeField] private Image slotImage;

    private void Start()
    {
        if (slotImage != null)
        {
            slotImage.enabled = false;
        }
    }

    public void GachaStart()
    {
        int hp;

        if (slotImage != null)
        {
            Debug.Log("ガチャスタート！");
            slotImage.enabled = true;
            PlayerHP.Instance.RecoverHP();

            hp = PlayerHP.Instance?.GetCurrentHP() ?? 0;
            Debug.Log("HP回復："+hp);
        }
    }
}
