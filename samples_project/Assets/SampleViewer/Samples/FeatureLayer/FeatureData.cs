using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArcGISLocationComponent))]
public class FeatureData : MonoBehaviour
{
    private double RayCastDistanceThreshold = 300000; 
    private double SpawnHeight = 10000;

    public ArcGISCameraComponent ArcGISCamera;
    public List<double> Coordinates = new List<double>();
    public Renderer FeatureRender;
    public List<string> properties = new List<string>();

    private void Start()
    {
        InvokeRepeating("DynamicScale", 2.0f, 0.5f);
        InvokeRepeating("SetOnGround", 0.1f, 0.3f);
    }

    private void DynamicScale()
    {
        var cameraLocationComponent = ArcGISCamera.GetComponent<ArcGISLocationComponent>();
        var scale = cameraLocationComponent.Position.Z * 25.0 / 20000;
        var featureHP = transform.GetComponent<HPTransform>();
        if (scale > 0)
        {
            featureHP.LocalScale = new Vector3((float)scale, (float)scale, (float)scale);   
        }
    }
    
    private void SetOnGround()
    {
        var cameraHP = ArcGISCamera.GetComponent<HPTransform>();
        var featureHP = transform.GetComponent<HPTransform>();
        var distance = (cameraHP.UniversePosition - featureHP.UniversePosition).ToVector3().magnitude;

        if (distance < RayCastDistanceThreshold)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, (float)SpawnHeight, 7))
            {
                // Modify the Stadiums altitude based off the raycast hit
                var locationComponent = transform.GetComponent<ArcGISLocationComponent>();
                double newHeight = locationComponent.Position.Z - hitInfo.distance;
                double stadiumLongitude = locationComponent.Position.X;
                double stadiumLatitude = locationComponent.Position.Y;
                ArcGISPoint position = new ArcGISPoint(stadiumLongitude, stadiumLatitude, newHeight,
                    locationComponent.Position.SpatialReference);
                locationComponent.Position = position;
                
                // The features were not being rendered until they are placed on the ground
                FeatureRender.transform.parent.gameObject.SetActive(true);
            }
        }
    }
}