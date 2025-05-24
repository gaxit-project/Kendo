using UnityEngine;

public class MobDestroy : MonoBehaviour
{
    public string targetTag = "Mob";
    public Gacha gacha;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            Destroy(other.gameObject);
            gacha.GachaStart();
        }
    }
}
