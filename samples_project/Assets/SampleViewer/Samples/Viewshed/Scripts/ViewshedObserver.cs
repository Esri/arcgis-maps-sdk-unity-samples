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

    // Start is called before the first frame update
    void Start()
    {
        SetupObserverCamera();
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector("_ViewshedObserverPosition", transform.position);//GetComponentInParent<HPTransform>().UniversePosition.ToVector3());
        Shader.SetGlobalMatrix("_ViewshedObserverViewProjMatrix", viewshedObserverCamera.projectionMatrix * viewshedObserverCamera.worldToCameraMatrix);
        Shader.SetGlobalTexture("_ViewshedObserverDepthTexture", viewshedObserverDepthTexture);
    }

    private void UpdateDepthTexture()
    {

        //viewshedObserverDepthTexture.
        //var cmd = new CommandBuffer();

        //RenderTargetIdentifier shadowmap = BuiltinRenderTextureType.CurrentActive;
        //viewshedObserverDepthTexture = new RenderTexture(2048, 2048, 16, RenderTextureFormat.ARGB32);
        //viewshedObserverDepthTexture.filterMode = FilterMode.Point;
        //cmd.SetShadowSamplingMode(shadowmap, ShadowSamplingMode.RawDepth);
        //var id = new RenderTargetIdentifier(viewshedObserverDepthTexture.depthBuffer);
        //cmd.Blit(shadowmap, id);
        //cmd.SetShadowSamplingMode(viewshedObserverCamera.targetTexture, ShadowSamplingMode.RawDepth);
        //cmd.SetGlobalTexture("_ViewshedObserverDepthTexture", viewshedObserverDepthTexture, RenderTextureSubElement.Depth);
        //viewshedObserverCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture, cmd);
        //viewshedObserverCamera.SetTargetBuffers(viewshedObserverDepthTexture.colorBuffer, viewshedObserverDepthTexture.depthBuffer);
    }

    private void SetupObserverCamera()
    {
        if(viewshedObserverCamera != null)
        {
            viewshedObserverCamera.depthTextureMode = DepthTextureMode.Depth;
            CreateViewshedObserverDepthTexture();
            //viewshedObserverCamera.GetUniversalAdditionalCameraData().requiresDepthTexture = true;
            //viewshedObserverCamera.GetUniversalAdditionalCameraData().requiresDepthOption = CameraOverrideOption.On;
            viewshedObserverCamera.targetTexture = viewshedObserverDepthTexture;
            
            
            Debug.Log("Created observer depth texture");
        }
    }

    private void CreateViewshedObserverDepthTexture()
    {
        int w = 2048;//Display.displays[viewshedObserverCamera.targetDisplay].renderingWidth;
        int h = 2048;//Display.displays[viewshedObserverCamera.targetDisplay].renderingHeight;
        
        
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
        //viewshedObserverColorTexture = new RenderTexture(w, h, 16, RenderTextureFormat.Default);
        //viewshedObserverDepthTexture = new RenderTexture(w, h, 24, RenderTextureFormat.Depth);
        //viewshedObserverCamera.SetTargetBuffers(viewshedObserverDepthTexture.colorBuffer, viewshedObserverDepthTexture.depthBuffer);
        viewshedObserverDepthTexture.filterMode = FilterMode.Point;
        //viewshedObserverDepthTexture.format = RenderTextureFormat.Depth;
        //viewshedObserverCamera.SetTargetBuffers(viewshedObserverDepthTexture.colorBuffer, viewshedObserverDepthTexture.depthBuffer);
        

        //viewshedObserverColorTexture = new RenderTexture(w, h, 0, RenderTextureFormat.Default);
        //viewshedObserverDepthTexture = new RenderTexture(w, h, 32, RenderTextureFormat.Depth);
        //viewshedObserverCamera.SetTargetBuffers(viewshedObserverColorTexture.colorBuffer, viewshedObserverDepthTexture.depthBuffer);
    }
}
