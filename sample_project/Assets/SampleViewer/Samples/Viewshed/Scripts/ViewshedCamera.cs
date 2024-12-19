// Copyright 2024 Esri.
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
    [SerializeField] private int depthWidth = 2048;
    [SerializeField] private int depthHeight = 2048;

    private RenderTexture depthTexture;
    private Camera viewshedCamera;
    private Vector3 lastPosition = Vector3.zero;

    private void Start()
    {
        if (viewshedCamera == null)
        {
            viewshedCamera = GetComponent<Camera>();
        }

        if (depthTexture == null)
        {
            CreateDepthTexture();
        }

        viewshedCamera.depthTextureMode = DepthTextureMode.Depth;
        viewshedCamera.targetTexture = depthTexture;

        viewshedMaterial.SetTexture("_DepthMap", viewshedCamera.targetTexture);
        //viewshedCamera.SetTargetBuffers(colorTexture.colorBuffer, depthTexture.depthBuffer);
    }

    private void CreateDepthTexture()
    {
        RenderTextureDescriptor renderTextureDescription = new RenderTextureDescriptor
        {
            width = depthWidth,
            height = depthHeight,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat,
            //colorFormat = RenderTextureFormat.RFloat,
            colorFormat = RenderTextureFormat.Depth,
            depthBufferBits = 32,
            //depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat_S8_UInt,
            msaaSamples = 1,
            dimension = TextureDimension.Tex2D,
            sRGB = false,
            volumeDepth = 1,
        };

        //depthTexture = new RenderTexture((int)width, (int)height, 0, rendertextureFormat, RenderTextureReadWrite.Linear);
        depthTexture = new RenderTexture(renderTextureDescription);
        depthTexture.filterMode = FilterMode.Bilinear;
        depthTexture.anisoLevel = 0;
        depthTexture.Create();
    }

    private void Update()
    {
        //Shader.SetGlobalMatrix("_ObserverViewProjection", viewshedCamera.projectionMatrix * viewshedCamera.worldToCameraMatrix);
        //Shader.SetGlobalFloat("_ViewshedDepthLimit", viewshedCamera.farClipPlane);

        if (viewshedMaterial == null)
        {
            return;
        }

        if (lastPosition == viewshedCamera.transform.position)
        {
            return;
        }

        var worldToCameraMatrix = viewshedCamera.worldToCameraMatrix;

        var renderType = GraphicsSettings.defaultRenderPipeline.GetType().ToString();

        // WorldToCameraMatrix SceneView Camera Matrix position is (0 0 0) in HDRP shaders.
        if (renderType == "UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset")
        {
            //worldToCameraMatrix.SetColumn(3, new Vector4(0, 0, 0, 1));
        }

        //Shader.SetGlobalMatrix("_ArcGISGlobalTerrainOcclusionViewProjMatrix", GL.GetGPUProjectionMatrix(viewshedCamera.projectionMatrix, true) * worldToCameraMatrix);

        viewshedMaterial.SetVector("_ViewshedCameraPosition", viewshedCamera.transform.position);
        viewshedMaterial.SetMatrix("_ViewProjectionMatrix", GL.GetGPUProjectionMatrix(viewshedCamera.projectionMatrix, true) * worldToCameraMatrix);

        //viewshedMaterial.SetTexture("_DepthMap", viewshedCamera.targetTexture);

        lastPosition = viewshedCamera.transform.position;

        print("ViewshedCamera.Update() called");
    }
}
