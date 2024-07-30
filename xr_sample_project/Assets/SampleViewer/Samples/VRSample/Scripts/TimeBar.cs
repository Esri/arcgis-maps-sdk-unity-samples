using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using TMPro;
public class TimeBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(timeText != null)
        {
            timeText.text = DateTime.Now.ToString("M/d/yyyy h:mm");
        }
    }
}
