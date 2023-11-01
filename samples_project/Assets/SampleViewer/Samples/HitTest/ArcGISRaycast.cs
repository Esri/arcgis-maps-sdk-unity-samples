// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;

public class ArcGISRaycast : MonoBehaviour
{
    [SerializeField] private InputAction inputAction;
    private const float offSet = 200f;
	
    public ArcGISMapComponent arcGISMapComponent;
    public ArcGISCameraComponent arcGISCamera;
    public Canvas canvas;
    public TextMeshProUGUI featureText;

    private void Start()
    {
	    canvas.enabled = false;
    }

    private void OnEnable()
    {
	    inputAction.Enable();
    }

    private void OnDisable()
    {
	    inputAction.Disable();
    }

    private void Update()
    {
	    if (inputAction.triggered)
	    {
		    if (!canvas.enabled)
		    {
			    canvas.enabled = true;
		    }
		    RaycastHit hit;
		    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		    if (Physics.Raycast(ray, out hit))
		    {
			    var arcGISRaycastHit = arcGISMapComponent.GetArcGISRaycastHit(hit);
			    var layer = arcGISRaycastHit.layer;
			    var featureId = arcGISRaycastHit.featureId;

			    if (layer != null && featureId != -1)
			    {
				    featureText.text = featureId.ToString();

				    var geoPosition = arcGISMapComponent.EngineToGeographic(hit.point);
				    var offsetPosition = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z + offSet, geoPosition.SpatialReference);

				    var rotation = arcGISCamera.GetComponent<ArcGISLocationComponent>().Rotation;
				    var location = canvas.GetComponent<ArcGISLocationComponent>();
				    location.Position = offsetPosition;
				    location.Rotation = rotation;
			    }
		    }
	    }
    }
}