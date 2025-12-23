// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

// A custom struct to hold data regarding ArcGIS map component positions

[Serializable]
public struct coordinates
{
    public string name;
    public float longitude;
    public float latitude;
    public float altitude;

    // Constructor
    public coordinates(string name, float longitude, float latitude, float altitude)
    {
        this.name = name;
        this.longitude = longitude;
        this.latitude = latitude;
        this.altitude = altitude;
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

    private int slotIndex = 0;
    [SerializeField] private CanvasGroup[] slots;

    // List of coordinates to set to ArcGIS Map origin, leading to 3D city scene layers collected by Esri

    [SerializeField] private List<coordinates> spawnLocations;

    private GameObject XROrigin;

    public void GoToLocation(int placeIndex)
    {
        GetLocation(placeIndex);
    }

    public void GoToRandomLocation()
    {
        GoToLocation(UnityEngine.Random.Range(0, spawnLocations.Count));
    }

    public void TraverseSlots(int indexMove)
    {
        if (slotIndex + indexMove < 0 || slotIndex + indexMove >= slots.Length)
        {
            return;
        }
        ToggleSlot(slots[slotIndex], false);

        slotIndex += indexMove;
        ToggleSlot(slots[slotIndex], true);

        backButton.interactable = slotIndex > 0;
        forwardButton.interactable = slotIndex < slots.Length - 1;
    }

    private void GetLocation(int index)
    {
        if (index >= 0 && index < spawnLocations.Count)
        {
            StartCoroutine(LoadIntoNewAreaWithFade(spawnLocations[index]));
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

        SetPlayerSpawn(Location.longitude, Location.latitude, Location.altitude);

        FadeScreen.Instance.FadeIn();

        // Reset previously changed booleans
        menuManager.GetComponent<VRMenuManager>().SetCurrentlyTeleporting(false);
        XROrigin.GetComponent<ActivateGrabRay>().currentlyTransporting = false;

        yield return new WaitForEndOfFrame();

        menuManager.GetComponent<VRMenuManager>().ToggleMenu(true);
    }

    private void SetNewArcGISMapOrigin(float longitude, float latitude)
    {
        // Confirm reference to map component before calling method within it
        arcGISMapComponent = arcGISMapComponent ? arcGISMapComponent : FindFirstObjectByType<ArcGISMapComponent>();
        arcGISMapComponent.OriginPosition = new ArcGISPoint(longitude, latitude, 0, ArcGISSpatialReference.WGS84());
    }

    private void SetPlayerSpawn(float longitude, float latitude, float altitude)
    {
        SetNewArcGISMapOrigin(longitude, latitude);

        // Confirm reference to XROrigin before calling method within it
        XROrigin = XROrigin ? XROrigin : FindFirstObjectByType<XROrigin>().gameObject;

        ArcGISLocationComponent playerLocation = XROrigin.GetComponent<ArcGISLocationComponent>();

        if (playerLocation)
        {
            playerLocation.Position = new ArcGISPoint(longitude, latitude, altitude, ArcGISSpatialReference.WGS84());
        }
    }

    private void Start()
    {
        // Cache private variables
        XROrigin = FindFirstObjectByType<XROrigin>().gameObject;
        arcGISMapComponent = FindFirstObjectByType<ArcGISMapComponent>();
        continuousMovement = XROrigin.GetComponent<ContinuousMovement>();

        menuManager = FindFirstObjectByType<VRMenuManager>().gameObject;
    }

    private void ToggleSlot(CanvasGroup slot, bool state)
    {
        slot.alpha = state ? 1 : 0;
        slot.interactable = state;
        slot.blocksRaycasts = state;
    }
}