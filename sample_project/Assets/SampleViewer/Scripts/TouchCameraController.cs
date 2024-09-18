using UnityEngine;

public class TouchCameraController : MonoBehaviour
{
    private void Awake()
    {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        DisableTouchJoysticks.instance.ToggleCanvas(true);
#endif
    }
}
