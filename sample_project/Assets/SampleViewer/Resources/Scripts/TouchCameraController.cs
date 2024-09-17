using Esri.ArcGISMapsSDK.Samples.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchCameraController : MonoBehaviour
{
    [SerializeField] private GameObject JoystickCanvas;

    private void Awake()
    {
#if UNITY_IOS || UNITY_ANDROID
        Instantiate(JoystickCanvas, new Vector3 (0,0,0), Quaternion.identity);
#endif
    }
}
