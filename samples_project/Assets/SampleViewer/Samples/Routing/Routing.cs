using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.ArcGISMapsSDK.Utils.Math;
using Esri.HPFramework;
using Esri.ArcGISMapsSDK.Components;

using UnityEngine;


public class Routing : MonoBehaviour
{
    public GameObject RouteMarker;

    private HPRoot hpRoot;
    private GameObject ActiveWayPoint;
    private ArcGISMapViewComponent arcGISMapViewComponent;

    void Start()
    {
        // Highlander Comment Here - We Expect Only 1 For Each of These
        hpRoot = FindObjectOfType<HPRoot>();
        arcGISMapViewComponent = FindObjectOfType<ArcGISMapViewComponent>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                var routeMarker = Instantiate(RouteMarker, hit.point, Quaternion.Euler(0.0f, 0.0f, 90.0f), arcGISMapViewComponent.transform);

                var geoPosition = HitToGeoPosition(hit);
                geoPosition.Z = 200;

                var locationComponent = routeMarker.GetComponent<ArcGISLocationComponent>();
                locationComponent.enabled = true;
                locationComponent.Position = geoPosition;
            }
        }
    }

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

}
