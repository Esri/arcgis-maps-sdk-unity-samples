// Copyright 2023 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Measure : MonoBehaviour
{
    private const float RaycastHeight = 5000f;
    private const float LineWidth = 5f;

    public GameObject Line;
    public TMP_Text GeodeticDistanceText;
    public GameObject LineMarker;
    public GameObject InterpolationMarker;
    public float InterpolationInterval = 100;
    public Button[] UnitButtons;
    public Button ClearButton;
    [SerializeField] float MarkerHeight = 200f;

    private ArcGISMapComponent arcGISMapComponent;
    private List<GameObject> featurePoints = new List<GameObject>();
    private Stack<GameObject> stops = new Stack<GameObject>();
    private double3 lastRootPosition;
    private double geodeticDistance = 0;
    private LineRenderer lineRenderer;
    private ArcGISLinearUnit currentUnit = new ArcGISLinearUnit(ArcGISLinearUnitId.Miles);
    private string unitText;

    private void Start()
    {
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();

        lineRenderer = Line.GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = LineWidth;
        lastRootPosition = arcGISMapComponent.GetComponent<HPRoot>().RootUniversePosition;

        ClearButton.onClick.AddListener(delegate
        {
            ClearLine();
        });

        UnitButtons[0].interactable = false;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                hit.point += new Vector3(0, MarkerHeight, 0);
                var lineMarker = Instantiate(LineMarker, hit.point, Quaternion.identity, arcGISMapComponent.transform);
                var thisPoint = arcGISMapComponent.EngineToGeographic(hit.point);

                var location = lineMarker.GetComponent<ArcGISLocationComponent>();
                location.enabled = true;
                location.Position = thisPoint;
                location.Rotation = new ArcGISRotation(0, 90, 0);

                if (stops.Count > 0)
                {
                    GameObject lastStop = stops.Peek();
                    var lastPoint = lastStop.GetComponent<ArcGISLocationComponent>().Position;

                    // Calculate distance from last point to this point.
                    geodeticDistance += ArcGISGeometryEngine.DistanceGeodetic(lastPoint, thisPoint, currentUnit, new ArcGISAngularUnit(ArcGISAngularUnitId.Degrees), ArcGISGeodeticCurveType.Geodesic).Distance;
                    UpdateDisplay();

                    featurePoints.Add(lastStop);

                    // Interpolate middle points between last point and this point.
                    Interpolate(lastStop, lineMarker, featurePoints);
                    featurePoints.Add(lineMarker);
                }

                // Add this point to stops and also to feature points where stop is user-drawed, and feature points is a collection of user-drawed and interpolated.
                stops.Push(lineMarker);
                RenderLine(ref featurePoints);
                RebaseLine();
            }
        }
    }

    private void Interpolate(GameObject start, GameObject end, List<GameObject> featurePoints)
    {

        float lengthOfLine = Vector3.Distance(start.transform.position, end.transform.position);
        float n = Mathf.Floor(lengthOfLine / InterpolationInterval) ;
        double dx = (end.transform.position.x - start.transform.position.x) / n;
        double dy = (end.transform.position.y - start.transform.position.y) / n;
        double dz = (end.transform.position.z - start.transform.position.z) / n;

        var previousInterpolation = start.transform.position;

        // Calculate n-1 intepolation points/n-1 segments because the last segment is already created by the end point.
        for (int i = 0; i < n - 1; i++)
        {
            GameObject nextInterpolation = Instantiate(InterpolationMarker, arcGISMapComponent.transform);

            // Calculate transform of nextInterpolation point.
            float nextInterpolationX = previousInterpolation.x + (float)dx;
            float nextInterpolationY = previousInterpolation.y + (float)dy;
            float nextInterpolationZ = previousInterpolation.z + (float)dz;
            nextInterpolation.transform.position = new Vector3(nextInterpolationX, nextInterpolationY, nextInterpolationZ);

            // Set default location component of nextInterpolation point.
            nextInterpolation.GetComponent<ArcGISLocationComponent>().enabled = true;
            nextInterpolation.GetComponent<ArcGISLocationComponent>().Rotation = new ArcGISRotation(0, 90, 0);

            var location = nextInterpolation.GetComponent<ArcGISLocationComponent>();
            location.Position = arcGISMapComponent.EngineToGeographic(nextInterpolation.transform.position);

            featurePoints.Add(nextInterpolation);
            previousInterpolation = nextInterpolation.transform.position;
        }
    }

    // Set height for point transform and location component.
    private void SetElevation(GameObject stop)
    {
        // Start the raycast in the air at an arbitrary to ensure it is above the ground.
        var position = stop.transform.position;
        var raycastStart = new Vector3(position.x, position.y + RaycastHeight, position.z);
        if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hitInfo))
        {
            var location = stop.GetComponent<ArcGISLocationComponent>();
            location.Position = arcGISMapComponent.EngineToGeographic(hitInfo.point);
            stop.transform.position = hitInfo.point;
        }
    }

    private void RenderLine(ref List<GameObject> featurePoints)
    {
        var allPoints = new List<Vector3>();

        foreach (var stop in featurePoints)
        {
            if (stop.transform.position.Equals(Vector3.zero))
            {
                Destroy(stop);
                continue;
            }
            allPoints.Add(stop.transform.position);
        }

        lineRenderer.positionCount = allPoints.Count;
        lineRenderer.SetPositions(allPoints.ToArray());
    }

    public void ClearLine()
    {
        foreach (var stop in featurePoints)
        {
            Destroy(stop);
        }

        featurePoints.Clear();
        stops.Clear();
        geodeticDistance = 0;
        UpdateDisplay();
        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
        }
    }

    private void RebaseLine()
    {
        var rootPosition = arcGISMapComponent.GetComponent<HPRoot>().RootUniversePosition;
        var delta = (lastRootPosition - rootPosition).ToVector3();
        if (delta.magnitude > 1) // 1km
        {
            if (lineRenderer != null)
            {
                Vector3[] points = new Vector3[lineRenderer.positionCount];
                lineRenderer.GetPositions(points);
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] += delta;
                }
                lineRenderer.SetPositions(points);
            }
            lastRootPosition = rootPosition;
        }
    }

    private void UnitChanged()
    {
        var newLinearUnit = new ArcGISLinearUnit(Enum.Parse<ArcGISLinearUnitId>(unitText));
        geodeticDistance = currentUnit.ConvertTo(newLinearUnit, geodeticDistance);
        currentUnit = newLinearUnit;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        GeodeticDistanceText.text = $"{Math.Round(geodeticDistance, 3)}";
    }

    public void SetUnitText(string text)
    {
        unitText = text;
    }

    public void UnitButtonOnClick()
    {
        UnitChanged();
    }

    // Keep unit buttons pressed after selection
    public void OnUnitButtonClicked(Button clickedButton)
    {
        int btnIndex = System.Array.IndexOf(UnitButtons, clickedButton);

        if (btnIndex == -1)
        {
            return;
        }

        foreach (Button btn in UnitButtons)
        {
            btn.interactable = true;
        }

        clickedButton.interactable = false;
    }
}