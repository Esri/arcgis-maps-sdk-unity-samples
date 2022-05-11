using System.Net.Http;
using System.Threading.Tasks;

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
    private GameObject ActiveWayPoint;
    private ArcGISMapViewComponent arcGISMapViewComponent;


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

        return arcGISMapViewComponent.RendererView.FromCartesianPosition(v3);
    }

    private async void HandleRoute(GeoPosition start, GeoPosition end)
    {
        var s = new JArray();
        var e = new JArray();
    }

}
