using UnityEngine;
using UnityEngine.UI;

public class DisableSampleButtonsForURP : MonoBehaviour
{
    private void Awake()
    {
#if !USE_HDRP_PACKAGE
        var sampleButton = GetComponent<Button>();
        sampleButton.interactable = false;
#endif
    }
}
