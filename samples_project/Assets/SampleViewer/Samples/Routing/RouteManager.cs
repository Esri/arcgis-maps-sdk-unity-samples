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

public class RouteManager : MonoBehaviour
{
    public GameObject RouteMarker;
    public GameObject RouteBreadcrumb;
    public GameObject Route;
    public GameObject RouteInfo;
    public string apiKey;

    private HPRoot hpRoot;
    private ArcGISMapComponent arcGISMapComponent;
    private float elevationOffset = 20.0f;
    private int StopCount = 2;
    private Queue<GameObject> stops = new Queue<GameObject>();
    private bool routing = false;
    private string routingURL = "https://route-api.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World/solve";
    private List<GameObject> breadcrumbs = new List<GameObject>();
    private List<GameObject> routeMarkers = new List<GameObject>();
    private LineRenderer lineRenderer;
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
    }

    async void Update()
    {
        // Only Create Marker when Shift is Held and Mouse is Clicked
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            if (routing)
            {
                Debug.Log("Please Wait for Results or Cancel");
                return;
            }

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                var routeMarker = Instantiate(RouteMarker, hit.point, Quaternion.identity, arcGISMapComponent.transform);

                var geoPosition = HitToGeoPosition(hit);

                var locationComponent = routeMarker.GetComponent<ArcGISLocationComponent>();
                locationComponent.enabled = true;
                locationComponent.Position = geoPosition;
                locationComponent.Rotation = new ArcGISRotation(0, 90, 0);

                stops.Enqueue(routeMarker);

                routeMarkers.Add(routeMarker);

                if (stops.Count > StopCount)
                    Destroy(stops.Dequeue());

                if (stops.Count == StopCount)
                {
                    routing = true;

                    string results = await FetchRoute(stops.ToArray());

                    if (results.Contains("error"))
                    {
                        DisplayError(results);
                    }
                    else
                    {
                        StartCoroutine(DrawRoute(results));
                    }

                    routing = false;
                }
            }
        }

        RebaseRoute();
    }
    
    /// <summary>
    /// Return GeoPosition Based on RaycastHit; I.E. Where the user clicked in the Scene.
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    private ArcGISPoint HitToGeoPosition(RaycastHit hit, float yOffset = 0)
    {
        var worldPosition = math.inverse(arcGISMapComponent.WorldMatrix).HomogeneousTransformPoint(hit.point.ToDouble3());

        var geoPosition = arcGISMapComponent.View.WorldToGeographic(worldPosition);
        var offsetPosition = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z + yOffset, geoPosition.SpatialReference);

        return GeoUtils.ProjectToSpatialReference(offsetPosition, new ArcGISSpatialReference(4326));
    }

    private void DisplayError(string error_text)
    {
        var error = JObject.Parse(error_text).SelectToken("error");
        var message = error.SelectToken("message");

        var tmp = RouteInfo.GetComponent<TextMeshProUGUI>();
        tmp.text = $"Error: {message}";
    }

    private async Task<string> FetchRoute(GameObject[] stops)
    {
        if (stops.Length != StopCount)
            return "";

        IEnumerable<KeyValuePair<string, string>> payload = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("stops", GetRouteString(stops)),
            new KeyValuePair<string, string>("returnRoutes", "true"),
            new KeyValuePair<string, string>("token", arcGISMapComponent.APIKey),
            new KeyValuePair<string, string>("f", "json"),
        };

        HttpContent content = new FormUrlEncodedContent(payload);

        HttpResponseMessage response = await client.PostAsync(routingURL, content);
        response.EnsureSuccessStatusCode();

        string results = await response.Content.ReadAsStringAsync();
        return results;
    }

    private string GetRouteString(GameObject[] stops)
    {
        ArcGISPoint startGP = stops[0].GetComponent<ArcGISLocationComponent>().Position;
        ArcGISPoint endGP = stops[1].GetComponent<ArcGISLocationComponent>().Position;

        string startString = $"{startGP.X}, {startGP.Y}";
        string endString = $"{endGP.X}, {endGP.Y}";
        
        return $"{startString};{endString}";
    }

    private GameObject CreateBreadCrumb(float lat, float lon)
    {
        GameObject breadcrumb = Instantiate(RouteBreadcrumb, arcGISMapComponent.transform);

        breadcrumb.name = "Breadcrumb";

        ArcGISLocationComponent location = breadcrumb.AddComponent<ArcGISLocationComponent>();
        location.Position = new ArcGISPoint(lat, lon, elevationOffset, new ArcGISSpatialReference(4326));

        return breadcrumb;
    }

    IEnumerator DrawRoute(string routeInfo)
    {
        ClearRoute();

        var info = JObject.Parse(routeInfo);
        var routes = info.SelectToken("routes");
        var features = routes.SelectToken("features");

        UpdateRouteInfo(features);

        foreach (var feature in features)
        {
            var geometry = feature.SelectToken("geometry");
            var paths = geometry.SelectToken("paths")[0];

            foreach(var path in paths)
            {
                var lat = (float)path[0];
                var lon = (float)path[1];

                breadcrumbs.Add(CreateBreadCrumb(lat, lon));

                yield return null;
                yield return null;
            }
        }

        SetBreadcrumbHeight();

        // need a frame for location component updates to occur
        yield return null;
        yield return null;

        RenderLine();
    }

    // Does a raycast to get the elevation for each point.  For routes covering long distances the raycast will only hit elevation that is actively loaded. If you are doing 
    // something like this the raycast needs to happen dynamically when the data is loaded. This can be accomplished by only raycasting for breadcrums within a distance of the camera.
    private void SetBreadcrumbHeight()
    {
        for (int i = 0; i < breadcrumbs.Count; i++)
        {
            SetElevation(breadcrumbs[i]);
        }
    }

    // Does a raycast to find the ground
    void SetElevation(GameObject breadcrumb)
    {
        // start the raycast in the air at an arbitrary to ensure it is above the ground
        var raycastHeight = 5000;
        var position = breadcrumb.transform.position;
        var raycastStart = new Vector3(position.x, position.y + raycastHeight, position.z);
        if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hitInfo))
        {
            var location = breadcrumb.GetComponent<ArcGISLocationComponent>();
            location.Position = HitToGeoPosition(hitInfo, elevationOffset);
        }
    }

    private void UpdateRouteInfo(JToken features)
    {
        var tmp = RouteInfo.GetComponent<TextMeshProUGUI>();

        var target_feature = features[0];
        var attributes = target_feature.SelectToken("attributes");

        var travel_time = (float)attributes.SelectToken("Total_TravelTime");
        var travel_text = string.Format("{0:0.00}", travel_time);

        tmp.text = $"{travel_text}";
    }

    private void ClearRoute()
    {
        foreach (var breadcrumb in breadcrumbs)
        {
            Destroy(breadcrumb);
        }

        breadcrumbs.Clear();

        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
        }
    }

    private void RenderLine() 
    {
        if (breadcrumbs.Count < 1)
            return;

        lineRenderer.widthMultiplier = 30;

        var allPoints = new List<Vector3>();

        foreach (var breadcrumb in breadcrumbs)
        {
            if (breadcrumb.transform.position.Equals(Vector3.zero))
            {
                Destroy(breadcrumb);
                continue;
            }

            allPoints.Add(breadcrumb.transform.position);
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

    public void ClearLineMarkers()
    {
        ClearRoute();

        foreach (var routeMaker in routeMarkers)
        {
            Destroy(routeMaker);
        }

        routeMarkers.Clear();
        stops.Clear();

        var tmp = RouteInfo.GetComponent<TextMeshProUGUI>();
        tmp.text = $"0";
    }

}
