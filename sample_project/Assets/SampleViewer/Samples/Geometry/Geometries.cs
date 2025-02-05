// Copyright 2023 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Geometries : MonoBehaviour
{
    [Header("-----------ArcGIS Components-----------")]
    private ArcGISCameraControllerComponent arcGISCameraControllerComponent;
    private ArcGISMapComponent arcGISMapComponent;
    private double3 lastRootPosition;

    [Header("-----------Line Objects-----------")]
    [SerializeField] public float InterpolationInterval = 100;
    [SerializeField] public GameObject InterpolationMarker;
    [SerializeField] public GameObject Line;
    [SerializeField] public GameObject LineMarker;
    [SerializeField] float MarkerHeight = 300f;
    private double calculation = 0;
    private List<GameObject> featurePoints = new List<GameObject>();
    private List<GameObject> lastToStartInterpolationPoints = new List<GameObject>();
    private const float LineWidth = 5f;
    private LineRenderer lineRenderer;
    private const float RaycastHeight = 5000f;
    private ArcGISPoint startPoint;
    private Stack<GameObject> stops = new Stack<GameObject>();

    [Header("-----------Units-----------")]
    private ArcGISAreaUnit currentAreaUnit = new ArcGISAreaUnit(ArcGISAreaUnitId.SquareMiles);
    private ArcGISLinearUnit currentLinearUnit = new ArcGISLinearUnit(ArcGISLinearUnitId.Miles);
    private string unitText;

    [Header("-----------State Management-----------")]
    private bool isDragging = false;
    private bool isEnvelopeMode = false;
    private bool isPolygonMode = false;
    private bool isPolylineMode = true;

    [Header("-----------UI-----------")]
    [SerializeField] public Button ClearButton;
    [SerializeField] public Button[] ModeButtons;
    [SerializeField] public TMP_Text result;
    [SerializeField] public Button[] UnitButtons;

    private InputManager inputManager;

    private void Awake()
    {
        inputManager = FindFirstObjectByType<InputManager>();
    }

    private void Start()
    {
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();
        arcGISCameraControllerComponent = FindObjectOfType<ArcGISCameraControllerComponent>();

        lineRenderer = Line.GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = LineWidth;
        lastRootPosition = arcGISMapComponent.GetComponent<HPRoot>().RootUniversePosition;

        ClearButton.onClick.AddListener(delegate
        {
            ClearLine();
        });

        ModeButtons[0].interactable = false;
        UnitButtons[0].interactable = false;
    }

    public void StartGeometry()
    {
        RaycastHit hit;

#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
#else
        Ray ray = Camera.main.ScreenPointToRay(inputManager.touchControls.Touch.TouchPosition.ReadValue<Vector2>());
#endif
        if (Physics.Raycast(ray, out hit))
        {
            hit.point += new Vector3(0, MarkerHeight, 0);
            var hitPoint = arcGISMapComponent.EngineToGeographic(hit.point);

            if (isEnvelopeMode)
            {
                startPoint = hitPoint;
                isDragging = true;
            }
            else
            {
                if (isPolygonMode)
                {
                    //clear interpolation points of last segments 
                    foreach (var point in lastToStartInterpolationPoints)
                    {
                        Destroy(point);
                        continue;
                    }
                    lastToStartInterpolationPoints.Clear();
                }

                var lineMarker = Instantiate(LineMarker, hit.point, Quaternion.identity, arcGISMapComponent.transform);
                var location = lineMarker.GetComponent<ArcGISLocationComponent>();
                location.enabled = true;
                location.Position = hitPoint;
                location.Rotation = new ArcGISRotation(0, 90, 0);

                if (stops.Count > 0)
                {
                    GameObject lastStop = stops.Peek();
                    var lastPoint = lastStop.GetComponent<ArcGISLocationComponent>().Position;
                    if (isPolylineMode)
                    {
                        // Calculate distance from last point to this point, and add to the total distance calculation.
                        calculation += ArcGISGeometryEngine.DistanceGeodetic(lastPoint, hitPoint, currentLinearUnit, new ArcGISAngularUnit(ArcGISAngularUnitId.Degrees), ArcGISGeodeticCurveType.Geodesic).Distance;
                        UpdateDisplay();
                    }
                    featurePoints.Add(lastStop);
                    // Interpolate middle points between last point and this point.
                    Interpolate(lastStop, lineMarker, featurePoints);
                    featurePoints.Add(lineMarker);
                }

                // Add this point to stops and also to feature points where stop is user-drawed, and feature points is a collection of user-drawed and interpolated.
                stops.Push(lineMarker);

                if (isPolygonMode)
                {
                    if (featurePoints.Count > 3)
                    {
                        Interpolate(lineMarker, featurePoints[0], lastToStartInterpolationPoints);
                    }
                    CreateandCalculatePolygon();
                }
                if (featurePoints.Count >= 2)
                    RenderLine(ref featurePoints);
                RebaseLine();
            }
        }
    }

    public void OnGeometryEnd()
    {
        if (isEnvelopeMode)
        {
            RaycastHit hit;
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
#else
            Ray ray = Camera.main.ScreenPointToRay(inputManager.touchControls.Touch.TouchPosition.ReadValue<Vector2>());
#endif

            if (Physics.Raycast(ray, out hit))
            {
                hit.point += new Vector3(0, MarkerHeight, 0);
                var endPoint = arcGISMapComponent.EngineToGeographic(hit.point);
                CreateAndCalculateEnvelope(startPoint, endPoint);
            }
            isDragging = false;
        }
    }

    private void UpdateDraggingVisualization()
    {
        RaycastHit hit;
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
#else
        Ray ray = Camera.main.ScreenPointToRay(inputManager.touchControls.Touch.TouchPosition.ReadValue<Vector2>());
#endif

        if (Physics.Raycast(ray, out hit))
        {
            hit.point += new Vector3(0, MarkerHeight, 0);
            var currentPoint = arcGISMapComponent.EngineToGeographic(hit.point);

            CreateAndCalculateEnvelope(startPoint, currentPoint);
        }
    }

    private void Update()
    {
        // Continuously update visual cue in update
        if (isEnvelopeMode && isDragging)
        {
            UpdateDraggingVisualization();
        }
    }

    private void CreateAndCalculateEnvelope(ArcGISPoint start, ArcGISPoint end)
    {
        var spatialReference = new ArcGISSpatialReference(3857);

        var southwestPoint = new ArcGISPoint(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), spatialReference);
        var northeastPoint = new ArcGISPoint(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y), spatialReference);

        var envelope = new ArcGISEnvelope(southwestPoint, northeastPoint);

        VisualizeEnvelope(envelope);

        calculation = ArcGISGeometryEngine.AreaGeodetic(envelope, currentAreaUnit, ArcGISGeodeticCurveType.Geodesic);
        UpdateDisplay();
    }

    private void VisualizeEnvelope(ArcGISEnvelope envelope)
    {
        ClearLine();

        var corners = new[]
        {
            new ArcGISPoint(envelope.XMin, envelope.YMin, envelope.SpatialReference),
            new ArcGISPoint(envelope.XMax, envelope.YMin, envelope.SpatialReference),
            new ArcGISPoint(envelope.XMax, envelope.YMax, envelope.SpatialReference),
            new ArcGISPoint(envelope.XMin, envelope.YMax, envelope.SpatialReference)
        };

        var markers = new List<GameObject>();

        foreach (var corner in corners)
        {
            var point = arcGISMapComponent.GeographicToEngine(corner);

            var marker = Instantiate(LineMarker, point, Quaternion.identity, arcGISMapComponent.transform);
            SetSurfacePlacement(marker, MarkerHeight);
            SetElevation(marker);

            markers.Add(marker);
            stops.Push(marker);
        }

        // Add the corners to featurePoints in correct order to form the envelope
        for (int i = 0; i < markers.Count; i++)
        {
            var currentMarker = markers[i];
            var nextMarker = markers[(i + 1) % markers.Count];

            featurePoints.Add(currentMarker);
            Interpolate(currentMarker, nextMarker, featurePoints);
        }

        // Close the rectangle by adding the first point at the end
        featurePoints.Add(markers[0]);

        RenderLine(ref featurePoints);
        RebaseLine();
    }

    private void SetSurfacePlacement(GameObject marker, double offset)
    {
        var locationComponent = marker.GetComponent<ArcGISLocationComponent>();
        locationComponent.SurfacePlacementMode = ArcGISSurfacePlacementMode.RelativeToGround;
        locationComponent.SurfacePlacementOffset = offset;
    }

    private void CreateandCalculatePolygon()
    {
        var spatialReference = new ArcGISSpatialReference(3857);
        var polygonBuilder = new ArcGISPolygonBuilder(spatialReference);

        foreach (var stop in stops)
        {
            var location = stop.GetComponent<ArcGISLocationComponent>().Position;
            polygonBuilder.AddPoint(location);
        }

        var polygon = polygonBuilder.ToGeometry();

        calculation = ArcGISGeometryEngine.AreaGeodetic(polygon, currentAreaUnit, ArcGISGeodeticCurveType.Geodesic);
        UpdateDisplay();
    }

    private void Interpolate(GameObject start, GameObject end, List<GameObject> featurePoints)
    {
        float lengthOfLine = Vector3.Distance(start.transform.position, end.transform.position);
        float n = Mathf.Floor(lengthOfLine / InterpolationInterval);
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
        // Start the raycast in the air at an arbitrary point to ensure it is above the ground.
        var position = stop.transform.position;
        var raycastStart = new Vector3(position.x, position.y + RaycastHeight, position.z);
        if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hitInfo))
        {
            var elevatedPosition = hitInfo.point + new Vector3(0, MarkerHeight, 0);

            stop.transform.position = elevatedPosition;

            var location = stop.GetComponent<ArcGISLocationComponent>();
            location.Position = arcGISMapComponent.EngineToGeographic(elevatedPosition);
        }
    }

    private void RenderLine(ref List<GameObject> featurePoints)
    {
        var allPoints = new List<Vector3>();

        foreach (var point in featurePoints)
        {
            if (point.transform.position.Equals(Vector3.zero))
            {
                Destroy(point);
                continue;
            }
            allPoints.Add(point.transform.position);
        }

        if (isPolygonMode)
        {
            //add the first point to line renderer so that polygon can be closed
            allPoints.Add(allPoints[0]);
            foreach (var point in lastToStartInterpolationPoints)
            {
                if (point.transform.position.Equals(Vector3.zero))
                {
                    Destroy(point);
                    continue;
                }
                allPoints.Add(point.transform.position);
            }
        }

        lineRenderer.positionCount = allPoints.Count;
        lineRenderer.SetPositions(allPoints.ToArray());
    }

    public void ClearLine()
    {
        foreach (var stop in stops)
        {
            Destroy(stop);
        }

        foreach (var point in featurePoints)
        {
            Destroy(point);
        }

        foreach (var point in lastToStartInterpolationPoints)
        {
            Destroy(point);
        }

        featurePoints.Clear();
        stops.Clear();

        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
        }

        calculation = 0;

        UpdateDisplay();
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

    public void ResetMode()
    {
        ClearLine();

        isPolygonMode = false;
        isEnvelopeMode = false;
        isPolylineMode = false;

        foreach (var button in ModeButtons)
        {
            button.interactable = true;
        }
    }

    public void SetPolylineMode()
    {
        ResetMode();

        isPolylineMode = true;
        ModeButtons[0].interactable = false;

        UnitButtons[0].GetComponentInChildren<TMP_Text>().text = "mi";
        UnitButtons[1].GetComponentInChildren<TMP_Text>().text = "ft";
        UnitButtons[2].GetComponentInChildren<TMP_Text>().text = "m";
        UnitButtons[3].GetComponentInChildren<TMP_Text>().text = "km";
    }

    public void SetPolygonMode()
    {
        ResetMode();

        isPolygonMode = true;
        ModeButtons[1].interactable = false;

        UnitButtons[0].GetComponentInChildren<TMP_Text>().text = "mi<sup>2</sup>";
        UnitButtons[1].GetComponentInChildren<TMP_Text>().text = "ft<sup>2</sup>";
        UnitButtons[2].GetComponentInChildren<TMP_Text>().text = "m<sup>2</sup>";
        UnitButtons[3].GetComponentInChildren<TMP_Text>().text = "km<sup>2</sup>";
    }

    public void SetEnvelopeMode()
    {
        ResetMode();

        isEnvelopeMode = true;
        ModeButtons[2].interactable = false;

        UnitButtons[0].GetComponentInChildren<TMP_Text>().text = "mi<sup>2</sup>";
        UnitButtons[1].GetComponentInChildren<TMP_Text>().text = "ft<sup>2</sup>";
        UnitButtons[2].GetComponentInChildren<TMP_Text>().text = "m<sup>2</sup>";
        UnitButtons[3].GetComponentInChildren<TMP_Text>().text = "km<sup>2</sup>";
    }

    private void UnitChanged()
    {
        if (isPolylineMode)
        {
            var newLinearUnit = new ArcGISLinearUnit(Enum.Parse<ArcGISLinearUnitId>(unitText));
            calculation = currentLinearUnit.ConvertTo(newLinearUnit, calculation);
            currentLinearUnit = newLinearUnit;
        }
        else
        {
            var newAreaUnit = new ArcGISAreaUnit(Enum.Parse<ArcGISAreaUnitId>(unitText));
            calculation = currentAreaUnit.ConvertTo(newAreaUnit, calculation);
            currentAreaUnit = newAreaUnit;
        }
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        result.text = $"{Math.Round(calculation, 3)}";
    }

    public void SetUnitText(string text)
    {
        unitText = isPolylineMode ? text : $"Square{text}";
    }

    public void UnitButtonOnClick()
    {
        UnitChanged();
    }

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