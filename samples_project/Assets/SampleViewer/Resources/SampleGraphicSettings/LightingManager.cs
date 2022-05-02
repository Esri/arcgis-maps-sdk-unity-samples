using UnityEngine;
using UnityEngine.Rendering;
using Esri.ArcGISMapsSDK.Components;

public class LightingManager : MonoBehaviour
{
    public GameObject HDRPLighting;
    public GameObject URPLighting;

    private bool ActivePipelineIsHDRP()
    {
        return GraphicsSettings.renderPipelineAsset.name.Contains("HDRP");
    }

    void Start()
    {
        if (ActivePipelineIsHDRP())
        {
            HDRPLighting.SetActive(true);
            URPLighting.SetActive(false);
        }
        else if (!ActivePipelineIsHDRP())
        {
            HDRPLighting.SetActive(false);
            URPLighting.SetActive(true);
        }
    }
}
