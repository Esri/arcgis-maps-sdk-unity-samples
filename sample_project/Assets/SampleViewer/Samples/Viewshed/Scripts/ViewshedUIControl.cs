using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewshedUIControl : MonoBehaviour
{
    public GameObject observer;
    public Slider fovSlider;
    public Slider distSlider;
    public Slider rotSlider;
    public Slider alphaSlider;


    private Camera observerCamera;

    void Awake()
    {
        observerCamera = observer.GetComponentInChildren<Camera>();

        fovSlider.onValueChanged.AddListener(value => observerCamera.fieldOfView = value);
        distSlider.onValueChanged.AddListener(value => observerCamera.farClipPlane = value);
        rotSlider.onValueChanged.AddListener(value => 
            {
                observer.transform.rotation = Quaternion.Euler(observer.transform.eulerAngles.x, value, observer.transform.eulerAngles.z);
            });
        alphaSlider.onValueChanged.AddListener(value => Shader.SetGlobalFloat("_ViewshedOpacity", value));
    }
}
