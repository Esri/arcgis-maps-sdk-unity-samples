using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using UnityEngine;

public class LocationMarker : MonoBehaviour
{
    private ArcGISGeospatialController cameraController;
    private ArcGISLocationComponent locationComponent;
    
    private void Awake()
    {
        cameraController = FindFirstObjectByType<ArcGISGeospatialController>();
        locationComponent = GetComponent<ArcGISLocationComponent>();
    }
    
    private void Update()
    {
        locationComponent.Position = new ArcGISPoint(cameraController.cameraGeospatialPose.Longitude,
            cameraController.cameraGeospatialPose.Latitude, ArcGISSpatialReference.WGS84());
        locationComponent.Rotation = new ArcGISRotation(cameraController.cameraGeospatialPose.EunRotation.eulerAngles.y,180,0);
    }
}
