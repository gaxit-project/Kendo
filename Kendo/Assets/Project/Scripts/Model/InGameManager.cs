using UnityEngine;
using Cysharp.Threading.Tasks;

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

    void Start()
    {
        Delay(1);
        SoundBGM.Instance.Play("main");
    }

    public bool GetKnockbackOnMobHit()
    {
        return knockbackOnMobHit;
    }

    public void SetKnockbackOnMobHit(bool value)
    {
        knockbackOnMobHit = value;
    }

    async void Delay(float delay)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
    }
}
