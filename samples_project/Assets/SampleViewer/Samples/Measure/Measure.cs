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

public class Measure : MonoBehaviour
{
    private const float RaycastHeight = 5000f;
    private const float LineWidth = 5f;

    public GameObject Line;
    public Text GeodeticDistanceText;
    public GameObject LineMarker;
    public GameObject InterpolationMarker;
    public float InterpolationInterval = 100;
    public Dropdown UnitDropdown;
    public Button ClearButton;
    [SerializeField] float MarkerHeight = 200f;

    private ArcGISMapComponent arcGISMapComponent;
    private List<GameObject> featurePoints = new List<GameObject>();
    private Stack<GameObject> stops = new Stack<GameObject>();
    private double3 lastRootPosition;
    private double geodeticDistance = 0;
    private LineRenderer lineRenderer;
    private ArcGISLinearUnit currentUnit = new ArcGISLinearUnit(ArcGISLinearUnitId.Meters);

    private void Start()
    {
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();

        lineRenderer = Line.GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = LineWidth;
        lastRootPosition = arcGISMapComponent.GetComponent<HPRoot>().RootUniversePosition;
        UnitDropdown.onValueChanged.AddListener(delegate
        {
            UnitChanged();
        });
        ClearButton.onClick.AddListener(delegate
        {
            ClearLine();
        });
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
        var startPoint = start.GetComponent<ArcGISLocationComponent>().Position;
        var endPoint = end.GetComponent<ArcGISLocationComponent>().Position;

        double d = ArcGISGeometryEngine.DistanceGeodetic(startPoint, endPoint, currentUnit, new ArcGISAngularUnit(ArcGISAngularUnitId.Degrees), ArcGISGeodeticCurveType.Geodesic).Distance;
        float n = Mathf.Floor((float)d / InterpolationInterval);
        double dx = (end.transform.position.x - start.transform.position.x) / n;
        double dy = (end.transform.position.y - start.transform.position.y) / n;
        double dz = (end.transform.position.z - start.transform.position.z) / n;

        var PreviousInterpolation = start.transform.position;

        // Calculate n-1 intepolation points/n-1 segments because the last segment is already created by the end point.
        for (int i = 0; i < n - 1; i++)
        {
            GameObject NextInterpolation = Instantiate(InterpolationMarker, arcGISMapComponent.transform);

            // Calculate transform of NextInterpolation point.
            float NextInterpolationX = PreviousInterpolation.x + (float)dx;
            float NextInterpolationY = PreviousInterpolation.y + (float)dy;
            float NextInterpolationZ = PreviousInterpolation.z + (float)dz;
            NextInterpolation.transform.position = new Vector3(NextInterpolationX, NextInterpolationY, NextInterpolationZ);

            // Set default location component of NextInterpolation point.
            NextInterpolation.GetComponent<ArcGISLocationComponent>().enabled = true;
            NextInterpolation.GetComponent<ArcGISLocationComponent>().Rotation = new ArcGISRotation(0, 90, 0);

            var location = NextInterpolation.GetComponent<ArcGISLocationComponent>();
            location.Position = arcGISMapComponent.EngineToGeographic(NextInterpolation.transform.position);

            featurePoints.Add(NextInterpolation);
            PreviousInterpolation = NextInterpolation.transform.position;
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
        var newLinearUnit = new ArcGISLinearUnit(Enum.Parse<ArcGISLinearUnitId>(UnitDropdown.options[UnitDropdown.value].text));
        geodeticDistance = currentUnit.ConvertTo(newLinearUnit, geodeticDistance);
        currentUnit = newLinearUnit;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        GeodeticDistanceText.text = $"Distance: {Math.Round(geodeticDistance, 3)} {currentUnit.LinearUnitId}";
    }
}