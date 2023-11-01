// Copyright 2023 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

// ArcGISMapsSDK

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Extent;
using Esri.GameEngine.Geometry;
using Esri.Unity;
using UnityEngine.SceneManagement;

// Unity

using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// System

using System;

// This sample code demonstrates the essential API calls to set up an ArcGISMap
// It covers generation and initialization for the necessary ArcGISMapsSDK game objects and components

// Render-In-Editor Mode
// The ExecuteAlways attribute allows a script to run both in editor and during play mode
// You can disable the run-in-editor mode functions by commenting out this attribute and reloading the scene
// NOTE: Hot reloading changes to an editor script doesn't always work. You'll need to restart the scene if you want your code changes to take effect
// You could write an editor script to reload the scene for you, but that's beyond the scope of this sample script
// See the Unity Hot Reloading documentation to learn more about hot reloading: https://docs.unity3d.com/Manual/script-Serialization.html
[ExecuteAlways]

public class APIMapCreator : MonoBehaviour
{
	private ArcGISMapComponent mapComponent;
	private ArcGISCameraComponent cameraComponent;
	public string APIKey = "";

	private ArcGISPoint geographicCoordinates = new ArcGISPoint(-74.054921, 40.691242, 3000, ArcGISSpatialReference.WGS84());

	// This sample event is used in conjunction with a Sample3DAttributes component
	// It passes a layer to a listener to process its attributes
	// The Sample3DAttributes component is not required, so you are free to remove this event and its invocations in both scripts
	// See ArcGISMapsSDK/Samples/Scripts/3DAttributesSample/Sample3DAttributesComponent.cs for more info
	public delegate void SetLayerAttributesEventHandler(Esri.GameEngine.Layers.ArcGIS3DObjectSceneLayer layer);
	public event SetLayerAttributesEventHandler OnSetLayerAttributes;
	// @@End(SetVar)

	// @@Start(Initialize)
	private void Start()
	{
		CreateArcGISMapComponent();
		CreateArcGISCamera();
		CreateSkyComponent();
		CreateViewStateLoggingComponent();
		CreateArcGISMap();
	}

	// The ArcGISMap component is responsible for setting the origin of the map
	// All geographically located objects need to be a parent of this object
	private void CreateArcGISMapComponent()
	{
		mapComponent = FindObjectOfType<ArcGISMapComponent>();

		if (!mapComponent)
		{
			var mapComponentGameObject = new GameObject("ArcGISMap");

			mapComponent = mapComponentGameObject.AddComponent<ArcGISMapComponent>();
		}

		mapComponent.OriginPosition = geographicCoordinates;
		mapComponent.MapType = Esri.GameEngine.Map.ArcGISMapType.Local;

		// To change the Map Type in editor, you can change the Map Type property of the Map component
		// When you change the Map Type, this event will trigger a call to rebuild the map
		// We only want to subscribe to this event once after the necessary game objects are added to the scene
		mapComponent.MapTypeChanged += new ArcGISMapComponent.MapTypeChangedEventHandler(CreateArcGISMap);
	}

	// ArcGIS Camera and Location components are added to a Camera game object to enable map rendering, player movement and tile loading
	private void CreateArcGISCamera()
	{
		cameraComponent = Camera.main.gameObject.GetComponent<ArcGISCameraComponent>();

		if (!cameraComponent)
		{
			var cameraGameObject = Camera.main.gameObject;

			// The Camera game object needs to be a child of the Map View game object in order for it to be correctly placed in the world
			cameraGameObject.transform.SetParent(mapComponent.transform, false);

			// We need to add an ArcGISCamera component
			cameraComponent = cameraGameObject.AddComponent<ArcGISCameraComponent>();

			// The Camera Controller component provides player movement to the Camera game object
			cameraGameObject.AddComponent<ArcGISCameraControllerComponent>();

			// The Rebase component adjusts the world origin to accound for 32 bit floating point precision issues as the camera moves around the scene
			cameraGameObject.AddComponent<ArcGISRebaseComponent>();
		}

		var cameraLocationComponent = cameraComponent.GetComponent<ArcGISLocationComponent>();

		if (!cameraLocationComponent)
		{
			// We need to add an ArcGISLocation component...
			cameraLocationComponent = cameraComponent.gameObject.AddComponent<ArcGISLocationComponent>();

			// ...and update its position and rotation in geographic coordinates
			cameraLocationComponent.Position = geographicCoordinates;
			cameraLocationComponent.Rotation = new ArcGISRotation(65, 68, 0);
		}
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

			if (!skyComponent.arcGISMapComponent)
			{
				skyComponent.arcGISMapComponent = mapComponent;
			}

			if (!skyComponent.CameraComponent)
			{
				skyComponent.CameraComponent = cameraComponent;
			}
		}
#endif
	}
	private void CreateViewStateLoggingComponent()
	{
		ArcGISViewStateLoggingComponent viewStateComponent = mapComponent.GetComponent<ArcGISViewStateLoggingComponent>();

		if (!viewStateComponent)
		{
			viewStateComponent = mapComponent.gameObject.AddComponent<ArcGISViewStateLoggingComponent>();
		}
	}

	// This function creates the actual ArcGISMap object that will use your data to create a map
	// This is the only function from this script that will get called again when the map type changes
	public void CreateArcGISMap()
	{
		if (SceneManager.GetActiveScene().name != "SampleViewer" && APIKey == "")
		{
			Debug.LogError("An API Key must be set on the SampleAPIMapCreator for content to load");
		}
		// Create the Map Document
		// You need to create a new ArcGISMap whenever you change the map type
		var map = new Esri.GameEngine.Map.ArcGISMap(mapComponent.MapType);

		// Set the Basemap
		map.Basemap = new Esri.GameEngine.Map.ArcGISBasemap(Esri.GameEngine.Map.ArcGISBasemapStyle.ArcGISImagery, APIKey);

		// Create the Elevation
		map.Elevation = new Esri.GameEngine.Map.ArcGISMapElevation(new Esri.GameEngine.Elevation.ArcGISImageElevationSource("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer", "Terrain 3D", ""));

		// Create ArcGIS layers and add them to the map
		var layer_1 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/nGt4QxSblgDfeJn9/arcgis/rest/services/UrbanObservatory_NYC_TransitFrequency/MapServer", "MyLayer_1", 1.0f, true, "");
		map.Layers.Add(layer_1);

		var layer_2 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/nGt4QxSblgDfeJn9/arcgis/rest/services/New_York_Industrial/MapServer", "MyLayer_2", 1.0f, true, "");
		map.Layers.Add(layer_2);

		var layer_3 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/4yjifSiIG17X0gW4/arcgis/rest/services/NewYorkCity_PopDensity/MapServer", "MyLayer_3", 1.0f, true, "");
		map.Layers.Add(layer_3);

		var buildingLayer = new Esri.GameEngine.Layers.ArcGIS3DObjectSceneLayer("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_NewYork_17/SceneServer", "Building Layer", 1.0f, true, "");
		map.Layers.Add(buildingLayer);

		// This call invokes an event used by the Sample3DAttributes component
		if (OnSetLayerAttributes != null)
		{
			OnSetLayerAttributes(buildingLayer);
		}

		// Remove a layer
		map.Layers.Remove(map.Layers.IndexOf(layer_3));

		// You can update an ArcGISLayer's name, opacity, and visibility without needing to rebuild the map
		// Update properties
		layer_1.Opacity = 0.9f;
		layer_2.Opacity = 0.6f;

		// If the map type is local, we will create a circle extent and attach it to the map's clipping area
		if (map.MapType == Esri.GameEngine.Map.ArcGISMapType.Local)
		{
			// Set this to true to enable an extent on the map component
			mapComponent.EnableExtent = true;

			var extentCenter = new Esri.GameEngine.Geometry.ArcGISPoint(-74.054921, 40.691242, 3000, ArcGISSpatialReference.WGS84());
			var extent = new ArcGISExtentCircle(extentCenter, 10000);

			try
			{
				map.ClippingArea = extent;
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}

		// We have completed setup and are ready to assign the ArcGISMap object to the View
		mapComponent.View.Map = map;

#if UNITY_EDITOR
		// The editor camera is moved to the position of the Camera game object when the map type is changed in editor
		if (!Application.isPlaying && SceneView.lastActiveSceneView != null)
		{
			SceneView.lastActiveSceneView.pivot = cameraComponent.transform.position;
			SceneView.lastActiveSceneView.rotation = cameraComponent.transform.rotation;
		}
#endif
	}
}
