using UnityEngine;

public class DisableTouchJoysticks : MonoBehaviour
{
    public static DisableTouchJoysticks instance;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public void ToggleCanvas(bool active)
    {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        gameObject.SetActive(active);
#endif
    }
}
