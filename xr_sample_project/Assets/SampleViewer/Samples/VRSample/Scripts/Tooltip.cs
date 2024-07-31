using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tooltip : MonoBehaviour
{
    [SerializeField] CanvasGroup tooltipUI;
    [SerializeField] private InputAction toggleMenuButton;
    bool activeState = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        toggleMenuButton.Enable();
    }

    private void OnDisable()
    {
        toggleMenuButton.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (toggleMenuButton.triggered)
        {
            EnableTooltip(!activeState);
        }
    }

    void EnableTooltip(bool state)
    {
        tooltipUI.alpha = state ? 1 : 0;
        tooltipUI.interactable = state;
        tooltipUI.blocksRaycasts = state;
        activeState = state;
    }
}
