using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoPointsLine : MonoBehaviour
{
    [Header("--------Line Point Transforms--------")]
    [SerializeField] private Transform firstPoint;
    [SerializeField] private Transform lastPoint;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }


    void Update()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, firstPoint.position);
        lineRenderer.SetPosition(1, lastPoint.position);
    }
}
