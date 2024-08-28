// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit;

public class ActivateGrabRay : MonoBehaviour
{
    public bool currentlyTransporting = false;

    [Header("--------Interactor Scripts--------")]
    [SerializeField] private XRDirectInteractor leftDirectGrab;
    [SerializeField] private XRDirectInteractor rightDirectGrab;

    [Header("-----------Ray Objects-----------")]
    [SerializeField] private XRRayInteractor rightGrabRay;
    [SerializeField] private XRRayInteractor leftGrabRay;
    [SerializeField] private XRInteractorLineVisual rightGrabLine;
    [SerializeField] private XRInteractorLineVisual leftGrabLine;

    private void Update()
    {
        // Only show the raycast grab lines when the player is not traveling to a new location and there is something in the raycast to interact with
        rightGrabRay.enabled = rightGrabLine.enabled = (!currentlyTransporting && rightDirectGrab.interactablesSelected.Count == 0);
        leftGrabRay.enabled = leftGrabLine.enabled = (!currentlyTransporting && leftDirectGrab.interactablesSelected.Count == 0);
    }
}