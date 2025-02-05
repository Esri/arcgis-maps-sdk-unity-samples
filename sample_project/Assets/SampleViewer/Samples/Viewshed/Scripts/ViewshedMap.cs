// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils;
using Esri.Unity;

using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class ViewshedMap : MonoBehaviour
{
    [SerializeField] private Material viewshedMaterial;
    public string APIKey = "";

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3);

        var mapComponent = FindFirstObjectByType<ArcGISMapComponent>();

        if (string.IsNullOrEmpty(APIKey))
        {
            APIKey = ArcGISProjectSettingsAsset.Instance.APIKey;
        }

        if (string.IsNullOrEmpty(APIKey))
        {
            Debug.LogError("An API Key must be set on the SampleAPIMapCreator or in the project settings for content to load");
        }

        var map = new Esri.GameEngine.Map.ArcGISMap(mapComponent.MapType);

        map.Basemap = new Esri.GameEngine.Map.ArcGISBasemap(Esri.GameEngine.Map.ArcGISBasemapStyle.ArcGISImagery, APIKey);

        map.Elevation = new Esri.GameEngine.Map.ArcGISMapElevation(new Esri.GameEngine.Elevation.ArcGISImageElevationSource("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer", "Terrain 3D", ""));

        var buildingLayer = new Esri.GameEngine.Layers.ArcGIS3DObjectSceneLayer("https://tiles.arcgis.com/tiles/z2tnIkrLQ2BRzr6P/arcgis/rest/services/SanFrancisco_Bldgs/SceneServer", "Building Layer", 1.0f, true, "");

        if (viewshedMaterial != null)
        {
            buildingLayer.MaterialReference = viewshedMaterial;
        }

        map.Layers.Add(buildingLayer);

        mapComponent.View.Map = map;
    }
}
