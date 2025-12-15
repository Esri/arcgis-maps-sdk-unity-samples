using UnityEngine;
using UnityEngine.Rendering;
using Esri.ArcGISMapsSDK.Components;

public class LightingManager : MonoBehaviour
{
    public GameObject HDRPLighting;
    public GameObject URPLighting;

    private bool ActivePipelineIsHDRP()
    {
        var asset = GraphicsSettings.defaultRenderPipeline;
        return asset != null && asset.name.Contains("HDRP");
    }

    void Start()
    {
        if (ActivePipelineIsHDRP())
        {
#if USE_HDRP_PACKAGE
            var HDRPLightingObject = Instantiate(HDRPLighting, transform);
            var Sky = HDRPLightingObject.GetComponentInChildren<ArcGISSkyRepositionComponent>();
            Sky.CameraComponent = FindFirstObjectByType<ArcGISCameraComponent>();
            Sky.arcGISMapComponent = FindFirstObjectByType<ArcGISMapComponent>();
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
