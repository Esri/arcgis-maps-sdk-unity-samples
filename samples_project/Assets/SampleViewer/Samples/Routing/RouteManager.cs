using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.ArcGISMapsSDK.Utils.Math;
using Esri.HPFramework;
using Esri.ArcGISMapsSDK.Components;

using UnityEngine;
using Newtonsoft.Json.Linq;


public class RouteManager : MonoBehaviour
{
    public GameObject RouteMarker;
    public string apiKey;

    private HPRoot hpRoot;
    private ArcGISMapViewComponent arcGISMapViewComponent;

    private int StopCount = 2;
    private Queue<GameObject> stops = new Queue<GameObject>();
    private bool routing = false;
    private string routingURL = "https://route-api.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World/solve";

    private HttpClient client = new HttpClient();

    void Start()
    {
        // We need HPRoot for the HitToGeoPosition Method
        hpRoot = FindObjectOfType<HPRoot>();

        // We need this ArcGISMapViewComponent for the FromCartesianPosition Method
        // defined on the ArcGISRendererView defined on the instnace of ArcGISMapViewComponent
        arcGISMapViewComponent = FindObjectOfType<ArcGISMapViewComponent>();
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
                geoPosition.Z = 200;  // TODO - Review hit.distance as shown in FeatureLayer example to "snap" to ground

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
                    DrawRoute(results);

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

    private void DrawRoute(string routeInfo)
    {
        var info = JObject.Parse(routeInfo);
        Debug.Log(info); // TODO - Parse & Draw with "ArcFactory" Logic
    }

}
