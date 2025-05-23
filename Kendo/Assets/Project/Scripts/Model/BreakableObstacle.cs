using UnityEngine;

public class BreakableObstacle : MonoBehaviour
{
    [SerializeField] private int maxHits = 3;
    [SerializeField] private Color[] hitColors; // 0:‰ŠúF, 1:1‰ñ–Ú, 2:2‰ñ–Ú
    private int currentHits = 0;

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null && hitColors.Length > 0)
        {
            rend.material.color = hitColors[0];
        }
    }

    public void Hit()
    {
        currentHits++;

        if (rend != null && currentHits < hitColors.Length)
        {
            rend.material.color = hitColors[currentHits];
        }

        if (currentHits >= maxHits)
        {
            gameObject.SetActive(false);
        }
    }
}
