using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLightRotation : MonoBehaviour
{
    private GameObject directionalLight;

    private void Start()
    {
        directionalLight = GameObject.Find("Directional Light");
    }

    public void SetDirectionalLightRotation(float value)
    {
        // Confirm reference to directional light before calling methods within it
        directionalLight = directionalLight ? directionalLight : GameObject.Find("Directional Light");
        if (directionalLight)
        {
            directionalLight.transform.rotation = Quaternion.Euler(value, directionalLight.transform.rotation.eulerAngles.y, directionalLight.transform.rotation.eulerAngles.z);
        }
    }
}
