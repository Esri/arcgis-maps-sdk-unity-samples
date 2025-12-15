// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ChangeMaterial : MonoBehaviour
{
    private void Start()
    {
        SkinnedMeshRenderer mapMan = GetComponent<SkinnedMeshRenderer>();
        Material mapManMat = null;

        if (GraphicsSettings.defaultRenderPipeline.name.Contains("HDRP"))
        {
            mapManMat = Resources.Load<Material>("MapmanForUnity/HDRP_Metallic_Standard/MapmanMaterialHDRP");
        }
        else if (GraphicsSettings.defaultRenderPipeline.name.Contains("URP"))
        {
            mapManMat = Resources.Load<Material>("MapmanForUnity/URP_Metallic_Standard/MapmanMaterialURP");
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
}
