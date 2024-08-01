// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;

using Unity.XR.CoreUtils;
using UnityEngine.UI;

// A custom struct to hold data regarding ArcGIS map component positions
public struct coordinates
{
    public float latitude;
    public float longitutde;
    public string name;
    public float playerSpawnX;
    public float playerSpawnY;
    public float playerSpawnZ;

    // Constructor
    public coordinates(string name, float longitutde, float latitude, float playerSpawnX, float playerSpawnY, float playerSpawnZ)
    {
        this.name = name;
        this.longitutde = longitutde;
        this.latitude = latitude;
        this.playerSpawnX = playerSpawnX;
        this.playerSpawnY = playerSpawnY;
        this.playerSpawnZ = playerSpawnZ;
    }
}

public class LocationSelector : MonoBehaviour
{
    private ArcGISMapComponent arcGISMapComponent;
    [SerializeField] private Button backButton;
    private ContinuousMovement continuousMovement;
    [SerializeField] private Button forwardButton;
    private GameObject menu;
    private GameObject menuManager;
    [SerializeField] private CanvasGroup[] slots;

    // List of coordinates to set to ArcGIS Map origin, leading to 3D city scene layers collected by Esri
    private List<coordinates> spawnLocations = new List<coordinates> {new coordinates("San Francisco", -122.4194f, 37.7749f, 0f, 150f, 0f), new coordinates("Girona, Spain", 2.8214f, 41.983f, 38f, 200f, 150f),
    new coordinates("Christchurch, New Zealand", 172.64f, -43.534f, -331.45f, 40.8f, 542.1f), new coordinates("New York", -74.006f, 40.7128f, -331.45f, 200f, 250.0f), new coordinates("Redlands", -117.196453f, 34.060143f, 1000.05f, 500f, 250.0f),
        new coordinates("Grand Canyon", -112.0962f, 36.1018f, 1500.0f, 1300f, 250.0f)};

    private GameObject XROrigin;

    public void BackSlot()
    {
        backButton.interactable = false;
        forwardButton.interactable = true;

        ToggleSlot(slots[0], true);
        ToggleSlot(slots[1], false);
    }

    public void ForthSlot()
    {
        backButton.interactable = true;
        forwardButton.interactable = false;

        ToggleSlot(slots[0], false);
        ToggleSlot(slots[1], true);
    }

    public void GoToRandomLocation()
    {
        coordinates spawnLocation = spawnLocations[UnityEngine.Random.Range(0, spawnLocations.Count)];
        switch (spawnLocation.name)
        {
            case "San Francisco":
                GoToSanFran();
                break;

            case "Christchurch, New Zealand":
                GoToChristchurchNewZealand();
                break;

            case "New York":
                GoToNewYork();
                break;

            case "Redlands":
                GoToRedlands();
                break;

            case "Grand Canyon":
                GoToGrandCanyon();
                break;

            default:
                GoToSpain();
                break;
        }
    }

    private void GetLocationByName(string locationName)
    {
        foreach (coordinates location in spawnLocations)
        {
            if (location.name == locationName)
            {
                StartCoroutine(LoadIntoNewAreaWithFade(location));
                return;
            }
        }
    }

    // Function to fade screen into static color, load into new area, then fade back out of the color
    private IEnumerator LoadIntoNewAreaWithFade(coordinates Location)
    {
        // Confirm menu is deactivated, and set bool to not allow the player to reactivate it
        menuManager.GetComponent<VRMenuManager>().SetCurrentlyTeleporting(true);

        // Set bool to deactivate grab rays while teleporting
        XROrigin.GetComponent<ActivateGrabRay>().currentlyTransporting = true;

        FadeScreen.Instance.FadeOut();

        // Wait for the fade out to finish before switching locations
        yield return new WaitForSeconds(FadeScreen.Instance.GetFadeDuration());

        SetPlayerSpawn(Location.longitutde, Location.latitude, Location.playerSpawnX, Location.playerSpawnY, Location.playerSpawnZ);

        FadeScreen.Instance.FadeIn();

        // Reset previously changed booleans
        menuManager.GetComponent<VRMenuManager>().SetCurrentlyTeleporting(false);
        XROrigin.GetComponent<ActivateGrabRay>().currentlyTransporting = false;

        if (!menu.activeSelf)
        {
            menu.SetActive(true);
        }
        menuManager.GetComponent<VRMenuManager>().RealignMenu();
    }

    private void SetNewArcGISMapOrigin(float longitutde, float latitude)
    {
        // Confirm reference to map component before calling method within it
        arcGISMapComponent = arcGISMapComponent ? arcGISMapComponent : FindObjectOfType<ArcGISMapComponent>();
        arcGISMapComponent.OriginPosition = new ArcGISPoint(longitutde, latitude, 0, ArcGISSpatialReference.WGS84());
    }

    private void SetPlayerSpawn(float longitutde, float latitude, float playerSpawnX, float playerSpawnY, float playerSpawnZ)
    {
        SetNewArcGISMapOrigin(longitutde, latitude);

        // Confirm reference to XROrigin before calling method within it
        XROrigin = XROrigin ? XROrigin : FindObjectOfType<XROrigin>().gameObject;
        XROrigin.transform.position = new Vector3(playerSpawnX, playerSpawnY, playerSpawnZ);
    }

    private void Start()
    {
        // Cache private variables
        XROrigin = FindObjectOfType<XROrigin>().gameObject;
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();
        continuousMovement = XROrigin.GetComponent<ContinuousMovement>();
        menu = GameObject.FindWithTag("VRCanvas");
        menuManager = FindObjectOfType<VRMenuManager>().gameObject;

        // Get a random set of coordinates from the list to spawn user in unique location
        GoToRandomLocation();
    }

    private void ToggleSlot(CanvasGroup slot, bool state)
    {
        slot.alpha = state ? 1 : 0;
        slot.interactable = state;
        slot.blocksRaycasts = state;
    }

    #region Public Teleportation Functions

    public void GoToChristchurchNewZealand()
    {
        GetLocationByName("Christchurch, New Zealand");

        // Confirm reference to continuousMovement component before calling method within it
        continuousMovement = continuousMovement ? continuousMovement : XROrigin.GetComponent<ContinuousMovement>();
        continuousMovement.SetSpeed(50f);
        continuousMovement.SetVerticalSpeed(15f);
    }

    public void GoToGrandCanyon()
    {
        GetLocationByName("Grand Canyon");

        // Confirm reference to continuousMovement component before calling method within it
        continuousMovement = continuousMovement ? continuousMovement : XROrigin.GetComponent<ContinuousMovement>();
        continuousMovement.SetSpeed(50f);
        continuousMovement.SetVerticalSpeed(15f);
    }

    public void GoToNewYork()
    {
        GetLocationByName("New York");

        // Confirm reference to continuousMovement component before calling method within it
        continuousMovement = continuousMovement ? continuousMovement : XROrigin.GetComponent<ContinuousMovement>();
        continuousMovement.SetSpeed(50f);
        continuousMovement.SetVerticalSpeed(15f);
    }

    public void GoToRedlands()
    {
        GetLocationByName("Redlands");

        // Confirm reference to continuousMovement component before calling method within it
        continuousMovement = continuousMovement ? continuousMovement : XROrigin.GetComponent<ContinuousMovement>();
        continuousMovement.SetSpeed(50f);
        continuousMovement.SetVerticalSpeed(15f);
    }

    public void GoToSanFran()
    {
        GetLocationByName("San Francisco");

        // Confirm reference to continuousMovement component before calling method within it
        continuousMovement = continuousMovement ? continuousMovement : XROrigin.GetComponent<ContinuousMovement>();
        continuousMovement.SetSpeed(50f);
        continuousMovement.SetVerticalSpeed(15f);
    }

    public void GoToSpain()
    {
        GetLocationByName("Girona, Spain");

        // Confirm reference to continuousMovement component before calling method within it
        continuousMovement = continuousMovement ? continuousMovement : XROrigin.GetComponent<ContinuousMovement>();
        continuousMovement.SetSpeed(50f);
        continuousMovement.SetVerticalSpeed(15f);
    }

    #endregion Public Teleportation Functions
}