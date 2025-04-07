// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class ViewshedCamera : MonoBehaviour
{
    [SerializeField] private Material viewshedMaterial;
    [SerializeField] private int depthWidth = 1024;
    [SerializeField] private int depthHeight = 1024;

    private RenderTexture depthTexture;
    private Camera viewshedCamera;
    private Vector3 lastViewshedCameraPosition = Vector3.zero;
    private Vector3 lastViewshedCameraRotation = Vector3.zero;

    private void Start()
    {
        viewshedCamera = GetComponent<Camera>();

        if (depthTexture == null)
        {
            CreateDepthTexture();
        }

        viewshedCamera.depthTextureMode = DepthTextureMode.Depth;
        viewshedCamera.targetTexture = depthTexture;

        Shader.SetGlobalTexture("_ArcGISViewshedDepthTex", viewshedCamera.targetTexture);
    }

    private void CreateDepthTexture()
    {
        RenderTextureDescriptor renderTextureDescription = new RenderTextureDescriptor
        {
            width = depthWidth,
            height = depthHeight,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
            colorFormat = RenderTextureFormat.Depth,
            depthBufferBits = 32,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat,
            msaaSamples = 1,
            dimension = TextureDimension.Tex2D,
            sRGB = false,
            volumeDepth = 1,
        };

        depthTexture = new RenderTexture(renderTextureDescription);
        depthTexture.format = RenderTextureFormat.Depth;
        depthTexture.filterMode = FilterMode.Bilinear;
        depthTexture.anisoLevel = 16;
        depthTexture.Create();
    }

    private void Update()
    {
        if (viewshedMaterial == null)
        {
            return;
        }

        if (lastViewshedCameraPosition == viewshedCamera.transform.position && lastViewshedCameraRotation == viewshedCamera.transform.eulerAngles)
        {
            return;
        }

        Shader.SetGlobalMatrix("_ArcGISViewshedViewProjectionMatrix", GL.GetGPUProjectionMatrix(viewshedCamera.projectionMatrix, true) * viewshedCamera.worldToCameraMatrix);
        Shader.SetGlobalFloat("_ArcGISViewshedFarPlane", viewshedCamera.farClipPlane);

        lastViewshedCameraPosition = viewshedCamera.transform.position;
        lastViewshedCameraRotation = viewshedCamera.transform.eulerAngles;
    }
}
