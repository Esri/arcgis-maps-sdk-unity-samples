using System.Collections;
using System.Collections.Generic;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using UnityEngine;
using Esri.HPFramework;

[RequireComponent(typeof(ArcGISLocationComponent))]
public class FeatureData : MonoBehaviour
{
    public List<double> coordinates = new List<double>();
    public List<string> properties = new List<string>();

    public Renderer FeatureRender;

    public ArcGISCameraComponent ArcGISCamera;

    private double SpawnHeight = 10000;
    public double RayCastDistanceThreshold = 300000;
    private bool OnGround = false;
    private int Counter = 0;
    public int UpdatesPerRayCast = 200;
    // Start is called before the first frame update
    public void Start()
    {
        // Starting the counter at a random value makes it so we won't do raycast calculation on the same tick
        Counter = Random.Range(0, UpdatesPerRayCast);
    }

    // Update is called once per frame
    public void Update()
    {
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
    
    private void SetOnGround()
    {
        var CameraHP = ArcGISCamera.GetComponent<HPTransform>();
        var StadiumHP = transform.GetComponent<HPTransform>();
        var Distance = (CameraHP.UniversePosition - StadiumHP.UniversePosition).ToVector3().magnitude;

        if (Distance < RayCastDistanceThreshold)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, (float)SpawnHeight))
            {
                // Modify the Stadiums altitude based off the raycast hit
                var LocationComponent = transform.GetComponent<ArcGISLocationComponent>();
                double NewHeight = LocationComponent.Position.Z - hitInfo.distance;
                double StadiumLongitude = LocationComponent.Position.X;
                double StadiumLatitude = LocationComponent.Position.Y;
                ArcGISPoint Position = new ArcGISPoint(StadiumLongitude, StadiumLatitude, NewHeight, LocationComponent.Position.SpatialReference);
                LocationComponent.Position = Position;

                OnGround = true;

                // The features were not being rendered until they are placed on the ground
                FeatureRender.transform.parent.gameObject.SetActive(true);
            }
        }
    }
}