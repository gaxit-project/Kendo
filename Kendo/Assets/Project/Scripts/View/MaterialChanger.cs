using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material knockbackMaterial;

    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            _renderer = GetComponentInChildren<Renderer>();
        }
    }

    public void SetKnockbackMaterial()
    {
        if (_renderer != null && knockbackMaterial != null)
        {
            _renderer.material = knockbackMaterial;
        }
    }

    public void ResetToNormalMaterial()
    {
        if (_renderer != null && normalMaterial != null)
        {
            _renderer.material = normalMaterial;
        }
    }
}
