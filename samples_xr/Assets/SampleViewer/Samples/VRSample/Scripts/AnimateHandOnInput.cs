using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    [Header("----------Input Actions----------")]
    [SerializeField] private InputActionProperty pinchAnimationAction;
    [SerializeField] private InputActionProperty gripAnimationAction;
    
    private Animator handAnimator;

    void Start()
    {
        handAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // Set Animation values from input actions
        handAnimator = handAnimator ? handAnimator : GetComponent<Animator>();

        if (handAnimator)
        {
            handAnimator.SetFloat("Trigger", pinchAnimationAction.action.ReadValue<float>());
            handAnimator.SetFloat("Grip", gripAnimationAction.action.ReadValue<float>());
        }

    }
}
