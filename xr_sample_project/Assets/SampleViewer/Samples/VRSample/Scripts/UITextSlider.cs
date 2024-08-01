// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UITextSlider : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    [SerializeField] private int continuousMovementValue = -1;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sliderText;
    [SerializeField] private SetTurnType turnTypeScript;

    private void ChangeSliderValue(float value)
    {
        sliderText.text = value.ToString();
        if (continuousMovementValue >= 0)
        {
            ContinuousMovement continuousMovement = FindAnyObjectByType<ContinuousMovement>();
            SetTurnType turnTypeScript = FindAnyObjectByType<SetTurnType>();

            if (continuousMovement != null)
            {
                switch (continuousMovementValue)
                {
                    case 0:
                        continuousMovement.SetSpeed(value);
                        break;

                    case 1:
                        continuousMovement.SetVerticalSpeed(value);
                        break;

                    case 2:
                        turnTypeScript.SetSmoothTurnSpeed(value);
                        break;

                    case 3:
                        turnTypeScript.SetSnapTurnSpeed(value);
                        break;
                }
            }
        }
    }

    private void EnableSlider(bool state)
    {
        if (continuousMovementValue == 2)
        {
            canvasGroup.interactable = state;
            canvasGroup.alpha = state ? 1 : 0.5f;
        }
        else
        {
            canvasGroup.interactable = !state;
            canvasGroup.alpha = !state ? 1 : 0.5f;
        }
    }

    private void OnDisable()
    {
        if (continuousMovementValue > 1)
        {
            turnTypeScript.OnTypeChanged -= EnableSlider;
        }
    }

    private void OnEnable()
    {
        if (continuousMovementValue > 1)
        {
            turnTypeScript.OnTypeChanged += EnableSlider;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        slider.onValueChanged.AddListener(ChangeSliderValue);

        canvasGroup = GetComponent<CanvasGroup>();

        sliderText.text = slider.value.ToString();
    }

    // Update is called once per frame
    private void Update()
    {
    }
}