// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;

public class TwoPointsLine : MonoBehaviour
{
    [Header("--------Line Point Transforms--------")]
    [SerializeField] private Transform firstPoint;

    [SerializeField] private Transform lastPoint;

    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, firstPoint.position);
        lineRenderer.SetPosition(1, lastPoint.position);
    }
}