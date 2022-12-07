using System.Collections;
using System.Collections.Generic;
using Esri.HPFramework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Viewshed observer component - used control the viewshed observer's parameters
public class ViewshedObserver : MonoBehaviour
{
    [SerializeField] private Camera viewshedObserverCamera;

    [SerializeField] private Slider cameraFOVSlider;
    [SerializeField] private Slider altitudeSlider;
    [SerializeField] private Slider rotationSlider;
    [SerializeField] private Slider opacitySlider;

    private bool isHDRP;

    private RenderTexture viewshedObserverDepthTexture;

    void Start()
    {
        isHDRP = GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition");
        SetupObserverCamera();
        InitializeSliderValues();
    }

    void Update()
    {
        Shader.SetGlobalVector("_ViewshedObserverPosition", transform.position);
        Shader.SetGlobalMatrix("_ViewshedObserverViewProjMatrix", viewshedObserverCamera.projectionMatrix * viewshedObserverCamera.worldToCameraMatrix);
        Shader.SetGlobalTexture("_ViewshedObserverDepthTexture", viewshedObserverDepthTexture);
        Shader.SetGlobalFloat("_ViewshedDepthThreshold", 0.00001f); 

        // Generate the scaledScreenParams for HDRP (set by Unity by default in URP)
        if(isHDRP)
        {
            Rect pixelRect = viewshedObserverCamera.pixelRect;
            Shader.SetGlobalVector("_ViewshedObserverScreenParams", new Vector4(pixelRect.width, pixelRect.height, 1.0f + 1.0f / pixelRect.width, 1.0f + 1.0f / pixelRect.height));            
        }
    }

    // Initialize the UI sliders to be in sync with the observer
    private void InitializeSliderValues()
    {
        cameraFOVSlider.value = viewshedObserverCamera.fieldOfView;
        altitudeSlider.value = transform.position.y;
        rotationSlider.value = transform.rotation.eulerAngles.y;
        opacitySlider.value = 0.5f;

        cameraFOVSlider.onValueChanged.AddListener(delegate {
            viewshedObserverCamera.fieldOfView = cameraFOVSlider.value;
        });
        altitudeSlider.onValueChanged.AddListener(delegate {
            transform.position = new Vector3(transform.position.x, altitudeSlider.value, transform.position.z);
        });
        rotationSlider.onValueChanged.AddListener(delegate {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotationSlider.value, transform.rotation.eulerAngles.z);
        });
        opacitySlider.onValueChanged.AddListener(delegate {
            Shader.SetGlobalFloat("_ViewshedOpacity", opacitySlider.value);
        });
        Shader.SetGlobalFloat("_ViewshedOpacity", opacitySlider.value);
    }

    // Setup/initialization related to the observer's camera
    private void SetupObserverCamera()
    {
        if(viewshedObserverCamera != null)
        {
            viewshedObserverCamera.depthTextureMode = DepthTextureMode.Depth;
            CreateViewshedObserverDepthTexture();
            viewshedObserverCamera.targetTexture = viewshedObserverDepthTexture;
        }
    }

    // Create the observer camera's depth texture (needed by main camera shader to render viewshed effect)
    private void CreateViewshedObserverDepthTexture()
    {
        int h = 2048;
        int w = (int)(viewshedObserverCamera.aspect * h);
        
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
        viewshedObserverDepthTexture.filterMode = FilterMode.Bilinear; 
    }
}
