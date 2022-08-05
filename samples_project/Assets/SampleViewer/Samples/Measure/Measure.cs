// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using UnityEngine;
using Unity.Mathematics;

/*
public struct UnitType
{
    public const double m = 1;
    public const double km = 0.001;
    public const double mi = 0.000621371;
    public const double ft = 3.28084;
}
*/
public enum UnitType
{
    m = 0,
    km = 1,
    mi = 2,
    ft=3
}

public class Measure : MonoBehaviour
{
    public GameObject Line;
    public string apiKey;
    public Text txt;
    private String unitTxt;
    public GameObject LineMarker;
    public GameObject InterpolationMarker;
    public double InterpolationInterval=100;
    public Dropdown UnitDropdown;
    public Button ClearButton;
    private HPRoot hpRoot;
    private ArcGISMapComponent arcGISMapComponent;
    private float elevationOffset = 20.0f;
    private GameObject FeaturePoint;
    private List<GameObject> featurePoints = new List<GameObject>();
    private Stack<GameObject> stops = new Stack<GameObject>();
    private GameObject lastStop;
    private ArcGISLocationComponent lastStopLocation;
    private Vector3 midPosition;
    private double3 lastRootPosition;
    private ArcGISPoint thisPoint;
    private ArcGISPoint lastPoint;
    private double distance;
    private LineRenderer lineRenderer;
    private List<LineRenderer> lines;
    private ArcGISSpatialReference spatialRef = new ArcGISSpatialReference(3857);
    private ArcGISLinearUnitId unit;
    private ArcGISAngularUnitId unitDegree = (ArcGISAngularUnitId)9102;
    UnitType currentUnit;

    void Start()
    {
        // We need HPRoot for the HitToGeoPosition Method
        hpRoot = FindObjectOfType<HPRoot>();

        // We need this ArcGISMapComponent for the FromCartesianPosition Method
        // defined on the ArcGISMapComponent.View
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();
        lineRenderer = Line.GetComponent<LineRenderer>();
        lastRootPosition = arcGISMapComponent.GetComponent<HPRoot>().RootUniversePosition;
        distance = 0;
        unit = (ArcGISLinearUnitId)9001;
        currentUnit = UnitType.m;
        unitTxt = " m";
        UnitDropdown.onValueChanged.AddListener(delegate {
            UnitChanged();
        });
        ClearButton.onClick.AddListener(delegate {
            ClearLine();
        });

       
    }
    
    void Update()
    {
 
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                var lineMarker = Instantiate(LineMarker, hit.point, Quaternion.identity, arcGISMapComponent.transform);

                var geoPosition = HitToGeoPosition(hit);

                lineMarker.GetComponent<ArcGISLocationComponent>().enabled = true;
                lineMarker.GetComponent<ArcGISLocationComponent>().Position = geoPosition;
                lineMarker.GetComponent<ArcGISLocationComponent>().Rotation = new ArcGISRotation(0, 90, 0);

                var thisPoint = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z, spatialRef);


                if (stops.Count > 0)
                {
                    featurePoints.Add(lineMarker);

                    lastStop = stops.Peek();
                    lastStopLocation = lastStop.GetComponent<ArcGISLocationComponent>();
                    lastPoint = new ArcGISPoint(lastStopLocation.Position.X, lastStopLocation.Position.Y, lastStopLocation.Position.Z, spatialRef);
                    //using degree
                    
                    distance = distance+ArcGISGeometryEngine.DistanceGeodetic(lastPoint, thisPoint, new ArcGISLinearUnit(unit), new ArcGISAngularUnit(unitDegree), ArcGISGeodeticCurveType.Geodesic).Distance;
                    txt.text = "Distance: "+ Math.Round(distance,3).ToString()+unitTxt;
                    

                    Insert(lastStop, lineMarker, featurePoints);
                    SetBreadcrumbHeight();
                    RenderLine(ref featurePoints);
                    RebaseLine();
                    
                }

                stops.Push(lineMarker);
                featurePoints.Add(lineMarker);




            }
        }

    }

    /// <summary>
    /// Return GeoPosition Based on RaycastHit; I.E. Where the user clicked in the Scene.
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>

    //Interpolate points between start and end to draw points on top of the terrain
    private void Insert(GameObject start, GameObject end, List<GameObject> featurePoints)
    {
        //calculating distance 
        ArcGISLocationComponent startLocation = start.GetComponent<ArcGISLocationComponent>();
        ArcGISLocationComponent endLocation = end.GetComponent<ArcGISLocationComponent>();
        ArcGISPoint startPoint = new ArcGISPoint(startLocation.Position.X, startLocation.Position.Y, startLocation.Position.Z, spatialRef);
        ArcGISPoint endPoint = new ArcGISPoint(endLocation.Position.X, endLocation.Position.Y, endLocation.Position.Z, spatialRef);
        double d = ArcGISGeometryEngine.DistanceGeodetic(startPoint, endPoint, new ArcGISLinearUnit((ArcGISLinearUnitId)9001), new ArcGISAngularUnit(unitDegree), ArcGISGeodeticCurveType.Geodesic).Distance;
        if (d < InterpolationInterval)
            return;


        GameObject mid = Instantiate(InterpolationMarker, arcGISMapComponent.transform);
        double midLocationComponentX = startLocation.Position.X + (endLocation.Position.X - startLocation.Position.X) / 2;
        double midLocaitonComponentY = startLocation.Position.Y + (endLocation.Position.Y - startLocation.Position.Y) / 2;
        mid.GetComponent<ArcGISLocationComponent>().enabled = true;
        mid.GetComponent<ArcGISLocationComponent>().Position = new ArcGISPoint(midLocationComponentX, midLocaitonComponentY, 0, spatialRef);
        mid.GetComponent<ArcGISLocationComponent>().Rotation = new ArcGISRotation(0, 90, 0);

        float midX = start.transform.position.x + (end.transform.position.x - start.transform.position.x) / 2;
        float midY = start.transform.position.y + (end.transform.position.y - start.transform.position.y) / 2;
        float midZ = start.transform.position.z + (end.transform.position.z - start.transform.position.z) / 2;
        midPosition = new Vector3(midX, midY, midZ);
        mid.transform.position = midPosition;


        featurePoints.Add(mid);
        Insert(start, mid, featurePoints);
        Insert(mid, end, featurePoints);

    }

    private ArcGISPoint HitToGeoPosition(RaycastHit hit, float yOffset = 0)
    {
        var worldPosition = math.inverse(arcGISMapComponent.WorldMatrix).HomogeneousTransformPoint(hit.point.ToDouble3());

        var geoPosition = arcGISMapComponent.View.WorldToGeographic(worldPosition);
        var offsetPosition = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z + yOffset, geoPosition.SpatialReference);

        return GeoUtils.ProjectToSpatialReference(offsetPosition, spatialRef);
    }

    private void SetBreadcrumbHeight()
    {
        for (int i = 0; i < featurePoints.Count; i++)
        {
            SetElevation(featurePoints[i]);
        }
    }

    // Does a raycast to find the ground
    void SetElevation(GameObject stop)
    {
        // start the raycast in the air at an arbitrary to ensure it is above the ground
        var raycastHeight = 5000;
        var position = stop.transform.position;
        var raycastStart = new Vector3(position.x, position.y + raycastHeight, position.z);
        if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hitInfo))
        {
            var location = stop.GetComponent<ArcGISLocationComponent>();
            location.Position = HitToGeoPosition(hitInfo, elevationOffset);
        }
    }



    private void RenderLine(ref List<GameObject> featurePoints)
    {
        //lineRenderer.positionCount = 0;
        lineRenderer.widthMultiplier = 5;

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
            Destroy(stop);
        featurePoints.Clear();
        stops.Clear();
        distance = 0;
        txt.text = "Distance: " + distance + unitTxt;
        if (lineRenderer)
            lineRenderer.positionCount = 0;

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

    void UnitChanged()
    {
        if (UnitDropdown.options[UnitDropdown.value].text == "Meters")
        {
            ArcGISLinearUnitId unitM = (ArcGISLinearUnitId)9001;
            unit = unitM;
            distance=ConvertUnits(distance, currentUnit, UnitType.m);
            currentUnit=UnitType.m;
            unitTxt = " m";
        }
        else if (UnitDropdown.options[UnitDropdown.value].text == "Kilometers")
        {
            ArcGISLinearUnitId unitKm = (ArcGISLinearUnitId)9036;
            unit = unitKm;
            distance=ConvertUnits(distance, currentUnit, UnitType.km);
            currentUnit = UnitType.km;
            unitTxt = " km";
        }
        else if (UnitDropdown.options[UnitDropdown.value].text == "Miles")
        {
            ArcGISLinearUnitId unitMi = (ArcGISLinearUnitId)9093;
            unit = unitMi;
            distance = ConvertUnits(distance, currentUnit, UnitType.mi);
            currentUnit = UnitType.mi;
            unitTxt = " mi";
        }
        else if (UnitDropdown.options[UnitDropdown.value].text == "Feet")
        {
            ArcGISLinearUnitId unitFt = (ArcGISLinearUnitId)9002;
            unit = unitFt;
            distance = ConvertUnits(distance, currentUnit, UnitType.ft);
            currentUnit = UnitType.ft;
            unitTxt = " ft";
        }
        txt.text = "Distance: " + Math.Round(distance, 3).ToString() + unitTxt;
        //UnitDropdown.interactable=false;

    }

    public static double ConvertUnits(double units, UnitType from, UnitType to)
    {
        double[][] factor =
        {
            new double[] { 1, 0.001, 0.000621371, 3.28084 },
            new double[] { 1000,   1,     0.621371,   3280.84},
            new double[] { 1609.344,     1.609344,       1,   5280},
            new double[] { 0.3048,    0.0003048,  0.00018939,    1}
        };
            
        return units * factor[(int)from][(int)to];
    }

}
