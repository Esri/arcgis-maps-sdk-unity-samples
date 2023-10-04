using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArcGISLocationComponent))]
public class FeatureData : MonoBehaviour
{
    private int Counter = 0;
    private bool OnGround = false;
    private double RayCastDistanceThreshold = 300000; 
    private double SpawnHeight = 10000;
    private int UpdatesPerRayCast = 200;

    
    public ArcGISCameraComponent ArcGISCamera;
    public List<double> Coordinates = new List<double>();
    public Renderer FeatureRender;
    public List<string> properties = new List<string>();

    private void Start()
    {
        // Starting the counter at a random value makes it so we won't do raycast calculation on the same tick
        Counter = Random.Range(0, UpdatesPerRayCast);
    }

    private void Update()
    {
        DynamicScale();
        // Check each object every UpdatesPerRayCast updates to see if it was placed on the ground yet
        if (OnGround)
        {
            return;
        }

        Counter++;
        if (Counter >= UpdatesPerRayCast)
        {
            Counter = 0;
            SetOnGround();
        }
    }

    private void DynamicScale()
    {
        var cameraLocationComponent = ArcGISCamera.GetComponent<ArcGISLocationComponent>();
        var scale = cameraLocationComponent.Position.Z * 25.0 / 20000;
        var featureHP = transform.GetComponent<HPTransform>();
        featureHP.LocalScale = new Vector3((float)scale, (float)scale, (float)scale);
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

                OnGround = true;

                // The features were not being rendered until they are placed on the ground
                FeatureRender.transform.parent.gameObject.SetActive(true);
            }
        }
    }
}