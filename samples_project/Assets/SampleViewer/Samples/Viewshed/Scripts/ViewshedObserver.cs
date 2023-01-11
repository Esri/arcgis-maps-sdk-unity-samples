using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewshedObserver : MonoBehaviour
{

    [SerializeField] private Camera viewshedObserverCamera;

    private RenderTexture viewshedObserverDepthTexture;

    void Awake()
    {
        if(viewshedObserverCamera == null)
        {
            viewshedObserverCamera = GetComponent<Camera>();
        }
    }

    void Start()
    {
        CreateObserverDepthTexture();
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalMatrix("_ObserverViewProjection", viewshedObserverCamera.projectionMatrix * viewshedObserverCamera.worldToCameraMatrix);
        Shader.SetGlobalTexture("_ObserverDepthTexture", viewshedObserverDepthTexture);
    }

    void CreateObserverDepthTexture()
    {
        int h = Camera.main.pixelHeight;
        int w = Camera.main.pixelWidth;

        RenderTextureDescriptor desc = new RenderTextureDescriptor
        {
            width = w,
            height = h,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat,
            depthBufferBits = 32,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat,
            msaaSamples = 1,
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
            sRGB = false,
            volumeDepth = 1
        };
        viewshedObserverDepthTexture = new RenderTexture(desc);
        viewshedObserverDepthTexture.filterMode = FilterMode.Bilinear;

        viewshedObserverCamera.depthTextureMode = DepthTextureMode.Depth;
        viewshedObserverCamera.targetTexture = viewshedObserverDepthTexture;
    }
}
