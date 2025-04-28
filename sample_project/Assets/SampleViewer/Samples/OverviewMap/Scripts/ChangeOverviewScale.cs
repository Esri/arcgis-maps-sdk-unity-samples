// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeOverviewScale : MonoBehaviour
{
    [SerializeField] private ArcGISLocationComponent cameraLocationComponent;
    [SerializeField] private HPTransform locationMarker;
    [SerializeField] private TMP_InputField mapSizeInput;
    [SerializeField] private int maxSize;
    [SerializeField] private int minSize;
    [SerializeField] private Camera overviewCamera;
    [SerializeField] private int scalar = 6;
    [SerializeField] private Toggle toggle;
    
    private void Start()
    {
        mapSizeInput.text = overviewCamera.orthographicSize.ToString();
        
        mapSizeInput.onSubmit.AddListener(delegate(string mapSize)
        {
            if (toggle.isOn)
            {
                return;
            }
            
            if (!float.TryParse(mapSize, out var size))
            {
                return;
            }
            
            overviewCamera.orthographicSize = size;
            SetLocationMarkerScale();
        });
        
        toggle.onValueChanged.AddListener(delegate(bool enabled)
        {
            mapSizeInput.transform.parent.transform.parent.gameObject.SetActive(!enabled);
            InvokeRepeating(nameof(SetMapSize), 0.0f, 0.01f);

            if (enabled)
            {
                return;
            }
            
            if (!float.TryParse(mapSizeInput.text, out var size))
            {
                return;
            }
                
            overviewCamera.orthographicSize = size;
            SetLocationMarkerScale();
            CancelInvoke(nameof(SetMapSize));
        });
    }

    private void SetLocationMarkerScale()
    {
        var locationMarkerScale = overviewCamera.orthographicSize * scalar;
        locationMarker.LocalScale = new Vector3(locationMarkerScale, locationMarkerScale, locationMarkerScale);
    }

    private void SetMapSize()
    {
        if (!toggle.isOn)
        {
            return;
        }

        overviewCamera.orthographicSize = Mathf.Clamp((float)cameraLocationComponent.Position.Z, minSize, maxSize);
        SetLocationMarkerScale();
    }
}
