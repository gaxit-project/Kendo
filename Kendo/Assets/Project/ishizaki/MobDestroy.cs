using UnityEngine;

public class MobDestroy : MonoBehaviour
{
    public string targetTag = "Mob";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            Destroy(other.gameObject);
        }
    }
}
