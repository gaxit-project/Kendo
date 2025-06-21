using UnityEngine;

public class BreakableObstacle : MonoBehaviour
{
    [SerializeField] private int maxHits = 3;
    [SerializeField] private Material[] hitMaterials; // Cube �ɐݒ肷��}�e���A��
    [SerializeField] private string targetChildName = "Cube"; // �}�e���A����ύX�������q�I�u�W�F�N�g��

    private int currentHits = 0;
    private Renderer targetRenderer;

    private void Awake()
    {
        Transform targetChild = transform.Find(targetChildName);
        if (targetChild != null)
        {
            targetRenderer = targetChild.GetComponent<Renderer>();
            if (targetRenderer != null && hitMaterials.Length > 0)
            {
                Debug.Log("Renderer found and material set.");
                targetRenderer.material = hitMaterials[0];
            }
            else
            {
                Debug.LogWarning("Renderer not found on child.");
            }
        }
        else
        {
            Debug.LogWarning($"Child with name '{targetChildName}' not found.");
        }
    }

    public void Hit()
    {
        currentHits++;
        Debug.Log($"BreakableObstacle hit! count: {currentHits}");

        if (targetRenderer != null && currentHits < hitMaterials.Length)
        {
            targetRenderer.material = hitMaterials[currentHits];
        }

        if (currentHits >= maxHits)
        {
            gameObject.SetActive(false);
        }
    }
}
