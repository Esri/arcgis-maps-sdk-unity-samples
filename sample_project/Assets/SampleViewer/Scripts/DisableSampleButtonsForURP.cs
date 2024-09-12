using UnityEngine;
using UnityEngine.UI;

public class DisableSampleButtonsForURP : MonoBehaviour
{
    private void Awake()
    {
#if USE_URP_PACKAGE
        var sampleButton = GetComponent<Button>();
        sampleButton.interactable = false;
#endif
    }
}
