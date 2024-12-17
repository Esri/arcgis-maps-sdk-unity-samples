using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ViewshedCamera : MonoBehaviour
{
    [SerializeField] private Camera viewshedCamera;
    [SerializeField] private int pixelHeight = 2048;
    [SerializeField] private int pixelWidth = 2048;

    private RenderTexture depthTexture;

    private void Start()
    {
        if (viewshedCamera == null)
        {
            viewshedCamera = GetComponent<Camera>();
        }

        RenderTextureDescriptor renderTextureDescription = new RenderTextureDescriptor
        {
            width = pixelWidth,
            height = pixelHeight,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat,
            depthBufferBits = 32,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat,
            msaaSamples = 1,
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
            sRGB = false,
            volumeDepth = 1
        };

        depthTexture = new RenderTexture(renderTextureDescription);
        depthTexture.filterMode = FilterMode.Bilinear;

        viewshedCamera.depthTextureMode = DepthTextureMode.Depth;
        viewshedCamera.targetTexture = depthTexture;
    }

    private void Update()
    {
        //Shader.SetGlobalMatrix("_ObserverViewProjection", viewshedCamera.projectionMatrix * viewshedCamera.worldToCameraMatrix);
        //Shader.SetGlobalFloat("_ViewshedDepthLimit", viewshedCamera.farClipPlane);
    }
}
