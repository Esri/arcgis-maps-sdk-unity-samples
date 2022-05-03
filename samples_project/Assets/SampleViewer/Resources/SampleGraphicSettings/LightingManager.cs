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
        HDRPLighting.SetActive(ActivePipelineIsHDRP());
        URPLighting.SetActive(!ActivePipelineIsHDRP());
    }
}
