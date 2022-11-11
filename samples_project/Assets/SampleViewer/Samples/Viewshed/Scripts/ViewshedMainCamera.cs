using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//Attach this script to the main camera in the Viewshed scene
public class ViewshedMainCamera : MonoBehaviour
{
    private Camera mainCamera;
    private bool isHDRP;
    void Start()
    {
        //Determine if the HDRP is being used
        isHDRP = GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition");
        mainCamera = GetComponent<Camera>();
    }

    void Update()
    {
        if(isHDRP)
        {
            //If HDRP is used, we need to pass the main camera's screen parameters since they are not set automatically
            Rect pixelRect = mainCamera.pixelRect;
            Shader.SetGlobalVector("_ViewshedMainCameraScreenParams", new Vector4(pixelRect.width, pixelRect.height, 1.0f + 1.0f / pixelRect.width, 1.0f + 1.0f / pixelRect.height));
        }
    }
}
