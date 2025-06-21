using UnityEngine;

public class FixedScreenScaleObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// ƒJƒƒ‰‚©‚ç‚Ì‹——£‚ğæ“¾
    /// </summary>
    private float GetDistance()
    {
        return (transform.position - Camera.main.transform.position).magnitude;
    }

    private void LateUpdate()
    {
        //transform.localScale = Vector3.one * _baseScale * GetDistance();
    }
}
