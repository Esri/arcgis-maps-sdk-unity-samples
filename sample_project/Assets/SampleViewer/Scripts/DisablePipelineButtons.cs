using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisablePipelineButtons : MonoBehaviour
{
    private void Awake()
    {
#if !USE_URP_PACKAGE || !USE_HDRP_PACKAGE
        gameObject.SetActive(false);
#endif
    }
}
