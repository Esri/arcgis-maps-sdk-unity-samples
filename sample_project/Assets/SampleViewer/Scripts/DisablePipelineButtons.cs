using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisablePipelineButtons : MonoBehaviour
{
    private void Awake()
    {
#if !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
}
