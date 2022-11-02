using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ViewshedMainCamera : MonoBehaviour
{
    private Camera mainCamera;
    private bool isHDRP;
    void Start()
    {
        isHDRP = GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition");
        mainCamera = GetComponent<Camera>();
    }

    void Update()
    {
        if(isHDRP)
        {
            Rect pixelRect = mainCamera.pixelRect;
            Shader.SetGlobalVector("_ViewshedMainCameraScreenParams", new Vector4(pixelRect.width, pixelRect.height, 1.0f + 1.0f / pixelRect.width, 1.0f + 1.0f / pixelRect.height));
        }
    }
}
