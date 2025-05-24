using UnityEngine;

public class BlackhallManager : MonoBehaviour
{
    [SerializeField] private string mobTag = "Mob";
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(mobTag))
        {
            Destroy(other.gameObject);
            GachaManager.Instance.Gacha();
        }
        else if (other.CompareTag(Player))
        {

        }
    }
}
