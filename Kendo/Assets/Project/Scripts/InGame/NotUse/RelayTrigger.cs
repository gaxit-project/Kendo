using UnityEngine;

public class RelayTrigger : MonoBehaviour
{
    private BreakCircle parent;

    private void Start()
    {
        parent = GetComponentInParent<BreakCircle>();
        if (parent == null)
        {
            Debug.LogWarning("親に BreakCircle が見つかりません");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (parent != null)
        {
            parent.OnHitRelayFromChild(other);
        }
    }
}
