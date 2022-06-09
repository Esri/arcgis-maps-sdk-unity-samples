// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Net.Http;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;

using UnityEngine;
using TMPro;

using Newtonsoft.Json.Linq;
using Unity.Mathematics;

public class measure : MonoBehaviour
{
    public GameObject Route;
    public GameObject RouteInfo;
    public string apiKey;
    public GameObject RouteMarker;
    private HPRoot hpRoot;
    private ArcGISMapComponent arcGISMapComponent;
    private GameObject lastStop;
    private float elevationOffset = 20.0f;
    private List<GameObject> featurePoints = new List<GameObject>();
    private Stack<GameObject> stops=new Stack<GameObject>();
    private double distance;
    private ArcGISPoint thisPoint;
    private ArcGISPoint lastPoint;
    private ArcGISLocationComponent lastStopLocation;
    private LineRenderer lineRenderer;
    public GameObject FeaturePoint;
    ArcGISSpatialReference spatialRef = new ArcGISSpatialReference(3857);
    private HttpClient client = new HttpClient();

    double3 lastRootPosition;

    void Start()
    {
        // We need HPRoot for the HitToGeoPosition Method
        hpRoot = FindObjectOfType<HPRoot>();

        // We need this ArcGISMapComponent for the FromCartesianPosition Method
        // defined on the ArcGISMapComponent.View
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();

        lineRenderer = Route.GetComponent<LineRenderer>();

        lastRootPosition = arcGISMapComponent.GetComponent<HPRoot>().RootUniversePosition;
        distance = 0;
       
    }

    void Update()
    {
        // Only Create Marker when Shift is Held and Mouse is Clicked
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                var routeMarker = Instantiate(RouteMarker, hit.point, Quaternion.identity, arcGISMapComponent.transform);

                var geoPosition = HitToGeoPosition(hit);

                routeMarker.GetComponent<ArcGISLocationComponent>().enabled = true;
                routeMarker.GetComponent<ArcGISLocationComponent>().Position = geoPosition;
                routeMarker.GetComponent<ArcGISLocationComponent>().Rotation = new ArcGISRotation(0, 90, 0);
                
                var thisPoint = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z, spatialRef);
                
               
                if (stops.Count > 0)
                {
                    lastStop = stops.Peek();
                    lastStopLocation = lastStop.GetComponent<ArcGISLocationComponent>();
                    lastPoint = new ArcGISPoint(lastStopLocation.Position.X, lastStopLocation.Position.Y, lastStopLocation.Position.Z, spatialRef);
                    distance = distance + ArcGISGeometryEngine.Distance(lastPoint, thisPoint);

                    featurePoints.Add(routeMarker);
                    featurePoints.Add(lastStop);
                    Insert(lastStop, routeMarker, ref featurePoints);
                    SetBreadcrumbHeight();
                    RenderLine();
                    
                    
                }

                stops.Push(routeMarker);
                featurePoints.Clear();
            }
        }
        if (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return))
        {
            featurePoints.Add(stops.Peek());
            SetBreadcrumbHeight();
            RenderLine();
            RebaseRoute();
        }
            
    }

    /// <summary>
    /// Return GeoPosition Based on RaycastHit; I.E. Where the user clicked in the Scene.
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>

    private void Insert(GameObject start,GameObject end, ref List<GameObject> featurePoints)
    {
        ArcGISLocationComponent startLocation = start.GetComponent<ArcGISLocationComponent>();
        ArcGISLocationComponent endLocation = end.GetComponent<ArcGISLocationComponent>();
        ArcGISPoint startPoint = new ArcGISPoint(startLocation.Position.X, startLocation.Position.Y, startLocation.Position.Z, spatialRef);
        ArcGISPoint endPoint = new ArcGISPoint(endLocation.Position.X, endLocation.Position.Y, endLocation.Position.Z, spatialRef);
        double d= ArcGISGeometryEngine.Distance(startPoint, endPoint);
        if (d < 50)
            return;
        
        GameObject mid = Instantiate(FeaturePoint, arcGISMapComponent.transform);
        double midX=(math.abs(startLocation.Position.X)- math.abs(endLocation.Position.X))/-2;
        double midY = (startLocation.Position.Y - endLocation.Position.Y)/2;
        //mid.GetComponent<ArcGISLocationComponent>().Position = GetMiddlePoint(startPoint.Y, startPoint.X, endPoint.Y, endPoint.X);
        mid.GetComponent<ArcGISLocationComponent>().Position = new ArcGISPoint(midX,midY,spatialRef);
        mid.GetComponent<ArcGISLocationComponent>().Rotation = new ArcGISRotation(0, 90, 0);
        featurePoints.Add(mid);
        Insert(start, mid, ref featurePoints);
        Insert(mid, end, ref featurePoints);

    }

    private ArcGISPoint GetMiddlePoint(double lat1, double lon1, double lat2, double lon2)
    {
        double dLon = (math.PI /180)*(lon2 - lon1);
        lat1 = (math.PI / 180) * lat1;
        lat2 = (math.PI / 180) * lat2;
        lon1 = (math.PI / 180) * lon1;

        double Bx = math.cos(lat2) * math.cos(dLon);
        double By = math.cos(lat2) * math.sin(dLon);
        double lat3 = math.atan2(math.sin(lat1) + math.sin(lat2), math.sqrt((math.cos(lat1) + Bx) * (math.cos(lat1) + Bx) + By * By));
        double lon3 = lon1 + math.atan2(By, math.cos(lat1) + Bx);

        double lat = (180 / math.PI) * lat3;
        double lon= (180 / math.PI) * lon3;
        ArcGISPoint midPoint = new ArcGISPoint(lon,lat,spatialRef);
        return midPoint;
    }

    private ArcGISPoint HitToGeoPosition(RaycastHit hit, float yOffset = 0)
    {
        var worldPosition = math.inverse(arcGISMapComponent.WorldMatrix).HomogeneousTransformPoint(hit.point.ToDouble3());

        var geoPosition = arcGISMapComponent.View.WorldToGeographic(worldPosition);
        var offsetPosition = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z + yOffset, geoPosition.SpatialReference);

        return GeoUtils.ProjectToSpatialReference(offsetPosition, spatialRef);
    }

    // Does a raycast to get the elevation for each point.  For routes covering long distances the raycast will only hit elevation that is actively loaded. If you are doing 
    // something like this the raycast needs to happen dynamically when the data is loaded. This can be accomplished by only raycasting for breadcrums within a distance of the camera.
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

    private void ClearRoute()
    {
        /*
        foreach (var stop in stops)
            Destroy(stop);

        stops.Clear();

        if (lineRenderer)
            lineRenderer.positionCount = 0;*/
    }
    
    private void RenderLine()
    {
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

    // The ArcGIS Rebase component
    private void RebaseRoute()
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

}
