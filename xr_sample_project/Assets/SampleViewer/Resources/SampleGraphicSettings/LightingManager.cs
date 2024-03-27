using UnityEngine;
using UnityEngine.Rendering;
using Esri.ArcGISMapsSDK.Components;

public class LightingManager : MonoBehaviour
{
    public GameObject HDRPLighting;
    public GameObject URPLighting;
    public bool IsCloudXRDemo;

    private bool ActivePipelineIsHDRP()
    {
        return GraphicsSettings.renderPipelineAsset.name.Contains("HDRP");
    }

    void Start()
    {
        if (IsCloudXRDemo || ActivePipelineIsHDRP())
        {
#if USE_HDRP_PACKAGE
            var HDRPLightingObject = Instantiate(HDRPLighting, transform);
            var Sky = HDRPLightingObject.GetComponentInChildren<ArcGISSkyRepositionComponent>();
            Sky.CameraComponent = FindObjectOfType<ArcGISCameraComponent>();
            Sky.arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();
            HDRPLightingObject.SetActive(true);
#endif
        }
        else
        {
            var URPLightingObject = Instantiate(URPLighting, transform);
            URPLightingObject.SetActive(true);

            Material skyMat = Resources.Load("Skybox/SkyboxLiteWarm", typeof(Material)) as Material;
            RenderSettings.skybox = skyMat;
        }
    }
}
