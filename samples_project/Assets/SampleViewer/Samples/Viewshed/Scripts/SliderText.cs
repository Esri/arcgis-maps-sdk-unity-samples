using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Simple text field updater that tracks the value of a slider object
public class SliderText : MonoBehaviour
{
    public string prefix = "";
    public Slider slider;
    public TextMeshProUGUI text;

    void Start()
    {
        if(slider != null)
        {
            slider.onValueChanged.AddListener(value => UpdateText());
        }
        UpdateText();
    }

    void UpdateText()
    {
        if(text != null && slider != null)
        {
            text.text = prefix + " " + slider.value.ToString("F");
        }
    }
}
