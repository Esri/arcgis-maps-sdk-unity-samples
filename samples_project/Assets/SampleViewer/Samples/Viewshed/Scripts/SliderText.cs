using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderText : MonoBehaviour
{
    public string prefix = "";

    public Slider slider;
    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        if(slider != null)
        {
            slider.onValueChanged.AddListener(delegate { UpdateText(); });
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
