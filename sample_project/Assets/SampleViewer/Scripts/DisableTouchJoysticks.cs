using UnityEngine;

public class DisableTouchJoysticks : MonoBehaviour
{
    public static DisableTouchJoysticks instance;

    private void Awake()
    {
        instance = this;
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        ToggleCanvas(false);
#endif
    }

    public void ToggleCanvas(bool active)
    {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        gameObject.SetActive(active);
#endif
    }
}
