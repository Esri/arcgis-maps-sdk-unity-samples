using System;
using System.Net.Http;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.ArcGISMapsSDK.Utils.Math;
using Esri.HPFramework;
using Esri.ArcGISMapsSDK.Components;

using UnityEngine;
using TMPro;

using Newtonsoft.Json.Linq;

public class RouteManager : MonoBehaviour
{
    public GameObject RouteMarker;
    public GameObject RouteBreadcrumb;
    public GameObject Route;
    public GameObject RouteInfo;
    public string apiKey;

    private HPRoot hpRoot;
    private ArcGISMapViewComponent arcGISMapViewComponent;

    private float elevation = 50.0f;

    private int StopCount = 2;
    private Queue<GameObject> stops = new Queue<GameObject>();
    private bool routing = false;
    private string routingURL = "https://route-api.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World/solve";
    private List<GameObject> breadcrumbs = new List<GameObject>();

    private LineRenderer lineRenderer;

    private HttpClient client = new HttpClient();

    void Start()
    {
        // We need HPRoot for the HitToGeoPosition Method
        hpRoot = FindObjectOfType<HPRoot>();

        // We need this ArcGISMapViewComponent for the FromCartesianPosition Method
        // defined on the ArcGISRendererView defined on the instnace of ArcGISMapViewComponent
        arcGISMapViewComponent = FindObjectOfType<ArcGISMapViewComponent>();

        lineRenderer = Route.GetComponent<LineRenderer>();
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
                var routeMarker = Instantiate(RouteMarker, hit.point, Quaternion.identity, arcGISMapViewComponent.transform);

                var geoPosition = HitToGeoPosition(hit);

                var locationComponent = routeMarker.GetComponent<ArcGISLocationComponent>();
                locationComponent.enabled = true;
                locationComponent.Position = geoPosition;
                locationComponent.Rotation = new Rotator(0, 90, 0);

                stops.Enqueue(routeMarker);

                if (stops.Count > StopCount)
                    Destroy(stops.Dequeue());

                if (stops.Count == StopCount)
                {
                    routing = true;

                    string results = await FetchRoute(stops.ToArray());
                    StartCoroutine(DrawRoute(results));

                    routing = false;
                }
            }
        }
    }
    
    /// <summary>
    /// Return GeoPosition Based on RaycastHit; I.E. Where the user clicked in the Scene.
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    private GeoPosition HitToGeoPosition(RaycastHit hit)
    {
        var rup = hpRoot.DRootUniversePosition;

        var v3 = new Vector3d(
            hit.point.x + rup.x, 
            hit.point.y + rup.y, 
            hit.point.z + rup.z
            );

        // Spatial Reference of geoPosition will be Determined Spatial Reference of layers currently being rendered
        var geoPosition = arcGISMapViewComponent.RendererView.FromCartesianPosition(v3);

        return GeoUtils.ProjectToWGS84(geoPosition);
    }

    private async Task<string> FetchRoute(GameObject[] stops)
    {
        if (stops.Length != StopCount)
            return "";

        IEnumerable<KeyValuePair<string, string>> payload = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("stops", GetRouteString(stops)),
            new KeyValuePair<string, string>("returnRoutes", "true"),
            new KeyValuePair<string, string>("token", apiKey),
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
        GeoPosition startGP = stops[0].GetComponent<ArcGISLocationComponent>().Position;
        GeoPosition endGP = stops[1].GetComponent<ArcGISLocationComponent>().Position;

        string startString = $"{startGP.X}, {startGP.Y}";
        string endString = $"{endGP.X}, {endGP.Y}";
        
        return $"{startString};{endString}";
    }

    private GameObject CreateBreadCrumb(float lat, float lon, float alt)
    {
        GameObject breadcrumb = Instantiate(RouteBreadcrumb, arcGISMapViewComponent.transform);

        breadcrumb.name = "Breadcrumb";

        ArcGISLocationComponent location = breadcrumb.AddComponent<ArcGISLocationComponent>();
        location.Position = new GeoPosition(lat, lon, alt, 4326);

        return breadcrumb;
    }

    IEnumerator DrawRoute(string routeInfo)
    {
        ClearRoute();

        var info    = JObject.Parse(routeInfo);
        var routes   = info.SelectToken("routes");
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

                breadcrumbs.Add(CreateBreadCrumb(lat, lon, elevation));

                yield return null;
            }
        }

        RenderLine();
    }

    private void UpdateRouteInfo(JToken features)
    {
        var tmp = RouteInfo.GetComponent<TextMeshProUGUI>();

        var target_feature = features[0];
        var attributes = target_feature.SelectToken("attributes");

        var travel_time = (float)attributes.SelectToken("Total_TravelTime");
        var travel_text = string.Format("{0:0.00}", travel_time);

        tmp.text = $"Time: {travel_text} Minutes";
    }

    private void ClearRoute()
    {
        foreach (var breadcrumb in breadcrumbs)
            Destroy(breadcrumb);

        breadcrumbs.Clear();

        if (lineRenderer)
            lineRenderer.positionCount = 0;
    }

    private void RenderLine() 
    {
        if (breadcrumbs.Count < 1)
            return;

        lineRenderer.widthMultiplier = 5;

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

}
