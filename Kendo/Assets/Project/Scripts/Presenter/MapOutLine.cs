using System.Collections.Generic;
using UnityEngine;

public class MapOutLine : MonoBehaviour
{
    public GameObject MapUp;
    public GameObject MapDown;
    public GameObject MapRight;
    public GameObject MapLeft;
    [SerializeField] Material lineMaterial;
    [SerializeField] Color lineColor = Color.white;
    [Range(1f, 5f)]
    [SerializeField] float lineWidth = 0.2f;

    LineRenderer lineRenderer;
    List<Vector3> corners;

    void Start()
    {
        // ���S�_���擾
        Vector3 up = MapUp.transform.position;
        Vector3 down = MapDown.transform.position;
        Vector3 right = MapRight.transform.position;
        Vector3 left = MapLeft.transform.position;

        float minX = left.x;
        float maxX = right.x;
        float minZ = down.z;
        float maxZ = up.z;

        // �l���𐄒�i���v���j
        corners = new List<Vector3>
        {
            new Vector3(minX, 0, maxZ), // ����
            new Vector3(maxX, 0, maxZ), // �E��
            new Vector3(maxX, 0, minZ), // �E��
            new Vector3(minX, 0, minZ), // ����
            new Vector3(minX, 0, maxZ)  // ����ɖ߂�
        };

        CreateLine();
        CreateColliders();
    }

    void CreateLine()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;

        lineRenderer.positionCount = corners.Count;
        lineRenderer.SetPositions(corners.ToArray());
    }

    void CreateColliders()
    {
        for (int i = 0; i < corners.Count - 1; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[i + 1];

            GameObject wall = new GameObject($"Wall_{i}");
            wall.tag = "Wall";
            wall.transform.position = (start + end) / 2;
            wall.transform.rotation = Quaternion.LookRotation(end - start);

            BoxCollider collider = wall.AddComponent<BoxCollider>();
            float length = Vector3.Distance(start, end);
            collider.size = new Vector3(0.2f, 1f, length);
        }
    }
}
