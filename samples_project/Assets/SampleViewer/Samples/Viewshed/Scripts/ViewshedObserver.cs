using System.Collections;
using System.Collections.Generic;
using Esri.HPFramework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal;

public class ViewshedObserver : MonoBehaviour
{
    [SerializeField] private Camera viewshedObserverCamera;

    private Material viewshedOverlayMaterial;

    [SerializeField] private RenderTexture viewshedObserverColorTexture;
    [SerializeField] private RenderTexture viewshedObserverDepthTexture;

    void Start()
    {
        SetupObserverCamera();
    }

    void Update()
    {
        Shader.SetGlobalVector("_ViewshedObserverPosition", transform.position);
        Shader.SetGlobalMatrix("_ViewshedObserverViewProjMatrix", viewshedObserverCamera.projectionMatrix * viewshedObserverCamera.worldToCameraMatrix);
        Shader.SetGlobalTexture("_ViewshedObserverDepthTexture", viewshedObserverDepthTexture);
        Shader.SetGlobalFloat("_ViewshedDepthThreshold", 0.000001f); //TODO: adjust with slider based on scene/eye distance
    }

    private void SetupObserverCamera()
    {
        if(viewshedObserverCamera != null)
        {
            viewshedObserverCamera.depthTextureMode = DepthTextureMode.Depth;
            CreateViewshedObserverDepthTexture();
            viewshedObserverCamera.targetTexture = viewshedObserverDepthTexture;
        }
    }

    private void CreateViewshedObserverDepthTexture()
    {
        int w = 2048;
        int h = 2048;
        
        RenderTextureDescriptor desc = new RenderTextureDescriptor
        {
            width = w,
            height = h,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat,
            depthBufferBits = 32,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat,
            msaaSamples = 1,
            dimension = TextureDimension.Tex2D,
            sRGB = false,
            volumeDepth = 1
        };
        viewshedObserverDepthTexture = new RenderTexture(desc);
        viewshedObserverDepthTexture.filterMode = FilterMode.Point; 
    }
}
