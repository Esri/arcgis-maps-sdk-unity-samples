using UnityEngine;
using UnityEngine.UI;

public class DisableSampleButtonForMobile : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        var sampleButton = GetComponent<Button>();
        sampleButton.interactable = false;
#endif
    }
}
