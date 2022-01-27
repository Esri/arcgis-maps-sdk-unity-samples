// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

// ArcGISMapsSDK

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Extent;
using Esri.GameEngine.Location;
using Esri.GameEngine.View.Event;
using Esri.Unity;

// Unity

using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// This sample code demonstrates the essential API calls to set up an ArcGISMap
// It covers generation and initialization for the necessary ArcGISMapsSDK game objects and components 

// Render-In-Editor Mode
// The ExecuteAlways attribute allows a script to run both in editor and during play mode
// You can disable the run-in-editor mode functions by commenting out this attribute and reloading the scene
// NOTE: Hot reloading changes to an editor script doesn't always work. You'll need to restart the scene if you want your code changes to take effect
// You could write an editor script to reload the scene for you, but that's beyond the scope of this sample script
// See the Unity Hot Reloading documentation to learn more about hot reloading: https://docs.unity3d.com/Manual/script-Serialization.html
[ExecuteAlways]
public class VRSampleAPI : MonoBehaviour
{
	public GameObject VRRig;
	public GameObject VRCamera;
	private ArcGISMapViewComponent arcGISMapViewComponent;
	private ArcGISCameraComponent cameraComponent;
	private ArcGISCameraControllerComponent cameraControllerComponent;
	private ArcGISLocationComponent cameraLocationComponent;
	private ArcGISRebaseComponent cameraRebaseComponent;

	private GeoPosition geographicCoordinates = new GeoPosition(-74.054921, 40.691242, 3000, (int)SpatialReferenceWkid.WGS84);

	// This sample event is used in conjunction with a Sample3DAttributes component
	// It passes a layer to a listener to process its attributes
	// The Sample3DAttributes component is not required, so you are free to remove this event and its invocations in both scripts
	// See ArcGISMapsSDK/Samples/Scripts/3DAttributesSample/Sample3DAttributesComponent.cs for more info
	public delegate void SetLayerAttributesEventHandler(Esri.GameEngine.Layers.ArcGIS3DModelLayer layer);
	public event SetLayerAttributesEventHandler OnSetLayerAttributes;

	private void Start()
	{
		CreateArcGISMapView();
		CreateArcGISCamera();
		CreateArcGISMapRenderer();
		CreateSkyComponent();
		SubscribeToViewStateEvents();
		CreateArcGISMap();
	}

	// The ArcGISMapView component is responsible for setting the origin of the map
	// All geographically located objects need to be a parent of this object
	private void CreateArcGISMapView()
	{
		arcGISMapViewComponent = FindObjectOfType<ArcGISMapViewComponent>();

		if (!arcGISMapViewComponent)
		{
			var mapViewGameObject = new GameObject("ArcGISMapView");
			arcGISMapViewComponent = mapViewGameObject.AddComponent<ArcGISMapViewComponent>();
		}

		arcGISMapViewComponent.Position = geographicCoordinates;
		arcGISMapViewComponent.ViewMode = Esri.GameEngine.Map.ArcGISMapType.Local;

		// To change the Map Type in editor, you can change the View Mode property of the Map View component
		// When you change the Map Type, this event will trigger a call to rebuild the map
		// We only want to subscribe to this event once after the necessary game objects are added to the scene
		arcGISMapViewComponent.ViewModeChanged += new ArcGISMapViewComponent.ViewModeChangedEventHandler(CreateArcGISMap);
	}

	// ArcGIS Camera and Location components are added to a Camera game object to enable map rendering, player movement and tile loading
	private void CreateArcGISCamera()
	{

		VRRig.transform.SetParent(arcGISMapViewComponent.transform, false);

		if(!VRCamera.GetComponent<ArcGISCameraComponent>())
			cameraComponent = VRCamera.AddComponent<ArcGISCameraComponent>();
		// The Camera Controller component provides player movement to the Camera game object
		if(!VRCamera.GetComponent<ArcGISCameraControllerComponent>())
			cameraControllerComponent=VRCamera.AddComponent<ArcGISCameraControllerComponent>();

		// The Rebase component adjusts the world origin to accound for 32 bit floating point precision issues as the camera moves around the scene
		if(!VRCamera.GetComponent<ArcGISRebaseComponent>())
			cameraRebaseComponent=VRCamera.AddComponent<ArcGISRebaseComponent>();
		if(!VRCamera.GetComponent <ArcGISLocationComponent>())
        {
			cameraLocationComponent = VRCamera.AddComponent<ArcGISLocationComponent>();
			cameraLocationComponent.Position = geographicCoordinates;
			cameraLocationComponent.Rotation = new Rotator(65, 68, 0);
		}
		

	}

	// The ArcGISRenderer component holds and updates the map's tiles
	private void CreateArcGISMapRenderer()
	{
		if (FindObjectOfType<ArcGISRendererComponent>())
		{
			return;
		}

		var rendererGameObject = new GameObject("ArcGISRenderer");
		rendererGameObject.transform.SetParent(arcGISMapViewComponent.transform, false);
		rendererGameObject.AddComponent<ArcGISRendererComponent>();
	}

	// An ArcGISSkyReposition component adjusts a UnityEngine.Rendering.Volume to account for changes made to the map type
	// They are only used with the HDRP graphics pipeline, so we've created the USE_HDRP_PACKAGE preprocessor to enables this section when you use HDRP
	// This code is not necessary if you are using URP or if you choose not to use a Volume in your HDRP scene
	private void CreateSkyComponent()
	{
#if USE_HDRP_PACKAGE
		// Add Sky Component
		var currentSky = FindObjectOfType<UnityEngine.Rendering.Volume>();
		if (currentSky)
		{
			// The ArcGISSkyReposition component changes the sky's parameters to account for differences between global and local map types
			ArcGISSkyRepositionComponent skyComponent = currentSky.gameObject.GetComponent<ArcGISSkyRepositionComponent>();

			if (!skyComponent)
			{
				skyComponent = currentSky.gameObject.AddComponent<ArcGISSkyRepositionComponent>();
			}

			if (!skyComponent.MapViewComponent)
			{
				skyComponent.MapViewComponent = arcGISMapViewComponent;
			}

			if (!skyComponent.CameraComponent)
			{
				skyComponent.CameraComponent = cameraComponent;
			}
		}
#endif
	}

	// You can subscribe to these events to show information about the view state and log warnings in the console
	// Logs usually describe events such as if the data is loading, if the data's state is changed, or if there's an error processing the data
	// You only need to subscribe to them once as long as you don't unsubscribe
	private void SubscribeToViewStateEvents()
	{
		// This event logs updates on the Elevation source data
		arcGISMapViewComponent.RendererView.ArcGISElevationSourceViewStateChanged += (object sender, ArcGISElevationSourceViewStateEventArgs data) =>
		{
			Debug.Log("ArcGISElevationSourceViewState " + data.ArcGISElevationSource.Name + " changed to : " + data.Status.ToString());
		};

		// This event logs changes to the layers' statuses
		arcGISMapViewComponent.RendererView.ArcGISLayerViewStateChanged += (object sender, ArcGISLayerViewStateEventArgs data) =>
		{
			Debug.Log("ArcGISLayerViewState " + data.ArcGISLayer.Name + " changed to : " + data.Status.ToString());
		};

		// This event logs the RendererView's overall status
		arcGISMapViewComponent.RendererView.ArcGISRendererViewStateChanged += (object sender, ArcGISRendererViewStateEventArgs data) =>
		{
			Debug.Log("ArcGISRendererViewState changed to : " + data.Status.ToString());
		};

		arcGISMapViewComponent.RendererView.ArcGISRendererViewSpatialReferenceChanged += (object sender, ArcGISRendererViewSpatialReferenceEventArgs data) =>
		{
			Debug.Log("ArcGISRendererViewSpatialReference changed to : " + data.SpatialReference.WKID.ToString());
		};
	}

	// This function creates the actual ArcGISMap object that will use your data to create a map
	// This is the only function from this script that will get called again when the map type changes
	public void CreateArcGISMap()
	{
		// API Key
		string apiKey = String.Empty;

		// Create the Map Document
		// You need to create a new ArcGISMap whenever you change the map type
		var arcGISMap = new Esri.GameEngine.Map.ArcGISMap(arcGISMapViewComponent.ViewMode);

		// Set the Basemap
		arcGISMap.Basemap = new Esri.GameEngine.Map.ArcGISBasemap("https://www.arcgis.com/sharing/rest/content/items/8d569fbc4dc34f68abae8d72178cee05/data", apiKey);

		// Create the Elevation
		arcGISMap.Elevation = new Esri.GameEngine.Map.ArcGISMapElevation(new Esri.GameEngine.Elevation.ArcGISImageElevationSource("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer", "Elevation", apiKey));

		// Create ArcGIS layers and add them to the map
		var layer_1 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/nGt4QxSblgDfeJn9/arcgis/rest/services/UrbanObservatory_NYC_TransitFrequency/MapServer", "MyLayer_1", 1.0f, true, apiKey);
		arcGISMap.Layers.Add(layer_1);

		var layer_2 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/nGt4QxSblgDfeJn9/arcgis/rest/services/New_York_Industrial/MapServer", "MyLayer_2", 1.0f, true, apiKey);
		arcGISMap.Layers.Add(layer_2);

		var layer_3 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/4yjifSiIG17X0gW4/arcgis/rest/services/NewYorkCity_PopDensity/MapServer", "MyLayer_3", 1.0f, true, apiKey);
		arcGISMap.Layers.Add(layer_3);

		var buildingLayer = new Esri.GameEngine.Layers.ArcGIS3DModelLayer("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_NewYork_17/SceneServer", "Building Layer", 1.0f, true, apiKey);
		arcGISMap.Layers.Add(buildingLayer);

		// This call invokes an event used by the Sample3DAttributes component
		if (OnSetLayerAttributes != null)
		{
			OnSetLayerAttributes(buildingLayer);
		}

		// Remove a layer
		arcGISMap.Layers.Remove(arcGISMap.Layers.IndexOf(layer_3));

		// You can update an ArcGISLayer's name, opacity, and visibility without needing to rebuild the map
		// Update properties
		layer_1.Opacity = 0.9f;
		layer_2.Opacity = 0.6f;

		// If the map type is local, we will create a circle extent and attach it to the map's clipping area
		if (arcGISMap.MapType == Esri.GameEngine.Map.ArcGISMapType.Local)
		{
			var extentCenter = new ArcGISPosition(-74.054921, 40.691242, 3000, Esri.ArcGISRuntime.Geometry.SpatialReference.WGS84());
			var extent = new ArcGISExtentCircle(extentCenter, 100000);

			try
			{
				arcGISMap.ClippingArea = extent;
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}

		// We have completed setup and are ready to assign the ArcGISMap object to the RendererView
		arcGISMapViewComponent.RendererView.Map = arcGISMap;
	}
	
}
