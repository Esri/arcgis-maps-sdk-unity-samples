using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UITextSlider : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sliderText;
    [SerializeField] private Slider slider;
    [SerializeField] private int continuousMovementValue = -1;

    [SerializeField] private SetTurnType turnTypeScript;
    CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        slider.onValueChanged.AddListener(ChangeSliderValue);

        canvasGroup = GetComponent<CanvasGroup>();

        sliderText.text = slider.value.ToString();
    }

    private void OnEnable()
    {
        if (continuousMovementValue > 1)
        {
            turnTypeScript.OnTypeChanged += EnableSlider;
        }
    }

    private void OnDisable()
    {
        if (continuousMovementValue > 1)
        {
            turnTypeScript.OnTypeChanged -= EnableSlider;
        }
    }

    void ChangeSliderValue(float value)
    {
        sliderText.text = value.ToString();
        if(continuousMovementValue >= 0)
        {
            ContinuousMovement continuousMovement = FindAnyObjectByType<ContinuousMovement>();
            SetTurnType turnTypeScript = FindAnyObjectByType<SetTurnType>();
            
            if(continuousMovement != null )
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

    void EnableSlider(bool state)
    {
        if(continuousMovementValue == 2)
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
