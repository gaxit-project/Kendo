using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [SerializeField] private bool knockbackOnMobHit = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool GetKnockbackOnMobHit()
    {
        return knockbackOnMobHit;
    }

    public void SetKnockbackOnMobHit(bool value)
    {
        knockbackOnMobHit = value;
    }
}
