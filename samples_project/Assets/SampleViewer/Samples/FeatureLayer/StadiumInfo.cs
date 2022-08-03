// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Collections.Generic;
using UnityEngine;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;


// This class holds information for each stadium, controls how they are rendered, and
// also is responsible for placing the object on the surface of the Earth using a raycast.
// For the raycast to properly work ArcGISMapViewComponent.UseMeshColliders has to be true.
public class StadiumInfo : MonoBehaviour
{
    [SerializeField]
    private List<string> Infos = new List<string>();

    public Renderer StadiumRender;
    public Renderer PinRender;

    public ArcGISCameraComponent ArcGISCamera;

    private double SpawnHeight = 10000;
    public double RayCastDistanceThreshold = 300000;
    private bool OnGround = false;
    private int Counter = 0;
    public int UpdatesPerRayCast = 200;

    public void SetInfo(string Info)
    {
        Infos.Add(Info);

        // Based on which leage team belongs to, either the national or american league, we will render the stadium differently
        // See StadiumMaterial.shadergraph for how this is being accomplished
        var StadiumMaterials = StadiumRender.materials;
        if (Info == "National")
        {
            foreach (var Material in StadiumMaterials)
            {
                Material.SetInt("NationalLeague", 1);
            }
            PinRender.material.SetInt("NationalLeague", 1);
        }
        else if (Info == "American")
        {
            foreach (var Material in StadiumMaterials)
            {
                Material.SetInt("NationalLeague", 0);
            }
            PinRender.material.SetInt("NationalLeague", 0);
        }

    }

    // Used to tell this object how high it was spawned so we can control the distance of the raycast
    public void SetSpawnHeight(double InSpawnHeight)
    {
        SpawnHeight = InSpawnHeight;
    }

    public void Start()
    {
        // Starting the counter at a random value makes it so we won't do raycast calculation on the same tick
        Counter = Random.Range(0, UpdatesPerRayCast);
    }

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

    // This Feature Layer does not contain information about the feature's altitude.
    // To account for this when we get within a certain distance. Cast a ray down
    // to find the height of the ground.
    // The reason we are checking within a distance is because we only stream data for what we are looking 
    // at so the hit test wouldn't work for objects that don't have loaded terrain underneath them
    // Another way to get the elevation would be to query/identify the elevation service you are using for each
    // feature to discover the altitude
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
                var StadiumLocationComponent = transform.GetComponent<ArcGISLocationComponent>();
                double NewHeight = StadiumLocationComponent.Position.Z - hitInfo.distance;
                double StadiumLongitude = StadiumLocationComponent.Position.X;
                double StadiumLatitude = StadiumLocationComponent.Position.Y;
                ArcGISPoint Position = new ArcGISPoint(StadiumLongitude, StadiumLatitude, NewHeight, StadiumLocationComponent.Position.SpatialReference);
                StadiumLocationComponent.Position = Position;

                OnGround = true;

                // The features were not being rendered until they are placed on the ground
                StadiumRender.transform.parent.gameObject.SetActive(true);
            }
        }
    }

}
