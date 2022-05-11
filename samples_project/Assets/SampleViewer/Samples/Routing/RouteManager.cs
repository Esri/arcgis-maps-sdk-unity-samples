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

    private HPRoot hpRoot;
    private ArcGISMapViewComponent arcGISMapViewComponent;

    private int StopCount = 2;
    private Queue<GameObject> stops = new Queue<GameObject>();
    private bool routing = false;


    void Start()
    {
        // We need HPRoot for the HitToGeoPosition Method
        hpRoot = FindObjectOfType<HPRoot>();

        // We need this ArcGISMapViewComponent for the FromCartesianPosition Method
        // defined on the ArcGISRendererView defined on the instnace of ArcGISMapViewComponent
        arcGISMapViewComponent = FindObjectOfType<ArcGISMapViewComponent>();
    }

    void Update()
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

                if (stops.Count == StopCount)
                    HandleRoute(stops.ToArray());
                else if (stops.Count > StopCount)
                {
                    Destroy(stops.Dequeue());
                    HandleRoute(stops.ToArray());
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

    private async void HandleRoute(GameObject[] stops)
    {
        routing = true;

        await Task.Delay(2000);

        routing = false;
    }

}
