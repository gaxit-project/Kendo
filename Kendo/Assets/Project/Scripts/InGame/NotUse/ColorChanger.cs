using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    private Renderer rend;
    private Color defaultColor;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            defaultColor = rend.material.color;
        }
    }

    public void SetColor(Color color)
    {
        if (rend != null)
        {
            rend.material.color = color;
        }
    }

    public void ResetColor()
    {
        if (rend != null)
        {
            rend.material.color = defaultColor;
        }
    }
}
