using UnityEngine;

public class BreakableObstacle : MonoBehaviour
{
    [SerializeField] private int maxHits = 3;
    [SerializeField] private Color[] hitColors; // 0:�����F, 1:1���, 2:2���
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
