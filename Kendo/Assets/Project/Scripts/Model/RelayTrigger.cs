using UnityEngine;

public class RelayTrigger : MonoBehaviour
{
    private BreakCircle parent;

    private void Start()
    {
        parent = GetComponentInParent<BreakCircle>();
        if (parent == null)
        {
            Debug.LogWarning("e‚É BreakCircle ‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ");
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
