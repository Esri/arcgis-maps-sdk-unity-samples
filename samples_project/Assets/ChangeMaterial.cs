using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ChangeMaterial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var pipeline = GraphicsSettings.defaultRenderPipeline;
        SkinnedMeshRenderer mapMan = GetComponent<SkinnedMeshRenderer>();
        Material mapManMat = null;

        if (GraphicsSettings.renderPipelineAsset.name.Contains("HDRP"))
        {
            mapManMat = Resources.Load("MapmanForUnity/HDRP_Metallic_Standard/MapmanMaterialHDRP", typeof(Material)) as Material;
        }
        else if (GraphicsSettings.renderPipelineAsset.name.Contains("URP"))
        {
            mapManMat = Resources.Load("MapmanForUnity/URP_Metallic_Standard/MapmanMaterialURP", typeof(Material)) as Material;
        }
        else
        {
            Debug.Log("Rendering pipeline must be HDRP or URP.");
        }

        if (mapManMat)
        {
            mapMan.material = mapManMat;
        }
        else
        {
            Debug.Log("Material missing.");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
