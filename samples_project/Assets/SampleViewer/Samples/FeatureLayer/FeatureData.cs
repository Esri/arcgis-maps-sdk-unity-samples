using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArcGISLocationComponent))]
public class FeatureData : MonoBehaviour
{
    private HPTransform cameraHP;
    private ArcGISLocationComponent cameraLocationComponent;
    private float distance;
    private HPTransform featureHP;
    private ArcGISLocationComponent locationComponent;
    private double RayCastDistanceThreshold = 300000;
    private double scale;
    private double SpawnHeight = 10000;
    
    public ArcGISCameraComponent ArcGISCamera;
    public List<double> Coordinates = new List<double>();
    public Renderer FeatureRender;
    public List<string> Properties = new List<string>();

    private void Start()
    {
        cameraLocationComponent = ArcGISCamera.GetComponent<ArcGISLocationComponent>();
        featureHP = transform.GetComponent<HPTransform>();
        cameraHP = ArcGISCamera.GetComponent<HPTransform>();
        featureHP = transform.GetComponent<HPTransform>();
        InvokeRepeating("DynamicScale", 2.0f, 0.5f);
        InvokeRepeating("SetOnGround", 0.1f, 0.3f);
    }

    private void DynamicScale()
    {
        //Based on trial and error, it was deduced to use the following number so that the scale is a 
        //nice size based on distance from the camera.
        scale = cameraLocationComponent.Position.Z * 0.00125f;

        if (scale > 0)
        {
            featureHP.LocalScale = new Vector3((float)scale, (float)scale, (float)scale);   
        }
    }
    
    private void SetOnGround()
    {
        distance = (cameraHP.UniversePosition - featureHP.UniversePosition).ToVector3().magnitude;

        if (distance < RayCastDistanceThreshold)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, (float)SpawnHeight, 7))
            {
                // Modify the Stadiums altitude based off the raycast hit
                locationComponent = transform.GetComponent<ArcGISLocationComponent>();
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