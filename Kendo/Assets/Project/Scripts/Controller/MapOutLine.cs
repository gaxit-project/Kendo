using System.Collections.Generic;
using UnityEngine;

public class MapOutLine : MonoBehaviour
{
    [SerializeField] Material lineMaterial;
    [SerializeField] Color lineColor;
    [Range(0.1f, 0.5f)]
    [SerializeField] float lineWidth;


    GameObject lineObj;
    LineRenderer lineRenderer;
    List<Vector3> linePoints;

    void Start()
    {
        //ListÇÃèâä˙âª
        linePoints = new List<Vector3>();
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _addLineObject();
        }
        if (Input.GetMouseButton(0))
        {
            _addPositionDataToLineRenderer();
        }
    }

    private void _addLineObject()
    {
        lineObj = new GameObject();
        lineObj.name = "Line";
        lineObj.AddComponent<LineRenderer>();
        lineObj.AddComponent<EdgeCollider2D>();
        lineObj.transform.SetParent(transform);
        _initRenderer();
    }

    private void _initRenderer()
    {
        lineRenderer = lineObj.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.material = lineMaterial;
        lineRenderer.material.color = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }

    private void _addPositionDataToLineRenderer()
    {
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);


        lineRenderer.positionCount += 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, worldPos);
        linePoints.Add(worldPos);
        //lineObj.GetComponent<EdgeCollider2D>().SetPoints(linePoints);
    }
}
