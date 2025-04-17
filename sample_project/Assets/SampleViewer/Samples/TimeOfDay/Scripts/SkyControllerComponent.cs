using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkyControllerComponent : MonoBehaviour
{
    public enum SkyMode
    {
        None,
        Animated,
        Simulated
    }

    public enum TimeMode
    {
        MilitaryTime,
        AMPM
    }

    [SerializeField] private SkyMode skyMode;
    [SerializeField] private TimeMode timeMode;

    [Header ("UI Elements")]
    [SerializeField] private Toggle militaryTimeToggle;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TextMeshProUGUI timeText;

    [Range(0f, 0.1f)]
    [SerializeField] private float speed = 0.1f;

    private double offset = -90;

    [Header("Time Variables")]
    [Range(0f, 24f)]
    [SerializeField] private double startTime = 6.0;

    [Range(0f, 24f)]
    [SerializeField] private double stopTime = 18.0;

    [Range(0f, 24f)]
    [SerializeField] private double time = 6.0;

    private void Start()
    {
        timeSlider.value = (float)time;

        if (militaryTimeToggle.isOn)
        {
            timeMode = TimeMode.MilitaryTime;
        }
        else
        {
            timeMode = TimeMode.AMPM;
        }

        timeSlider.onValueChanged.AddListener(delegate
        {
            time = timeSlider.value;
            transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, (float)RotateSky());
        });

        militaryTimeToggle.onValueChanged.AddListener(delegate
        {
            if (militaryTimeToggle.isOn)
            {
                timeMode = TimeMode.MilitaryTime;
            }
            else
            {
                timeMode = TimeMode.AMPM;
            }
        });
    }

    private void Update()
    {
        CalculateTime();

        switch (skyMode)
        {
            case SkyMode.None:
                {
                    break;
                }
            case SkyMode.Animated:
                {
                    time += speed;

                    if (time >= 24.0)
                    {
                        time = 0.0f;
                    }
                    else
                    {
                        if (time >= stopTime)
                        {
                            skyMode = SkyMode.None;
                        }
                        else
                        {
                            transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, (float) RotateSky());
                        }
                    }

                    break;
                }
            case SkyMode.Simulated:
                {
                    time += speed;

                    if (time >= 24.0)
                    {
                        time = 0.0f;
                    }
                    else
                    {
                        transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, (float) RotateSky());
                    }

                    break;
                }
        }
    }

    private double RotateSky()
    {
        return time / 24 * 360 + offset;
    }

    private void CalculateTime()
    {
        if (timeMode == TimeMode.MilitaryTime)
        {
            TimeSpan timeSpan = TimeSpan.FromHours(time);
            var hours = timeSpan.Hours < 10 ? string.Format("0" + timeSpan.Hours) : timeSpan.Hours.ToString();
            var minutes = timeSpan.Minutes < 10 ? string.Format("0" + timeSpan.Minutes) : timeSpan.Minutes.ToString();
            timeText.text = string.Format(hours + ":" + minutes);
        }
        else
        {
            if (time <= 1)
            {
                var newTime = time + 12;
                TimeSpan timeSpan = TimeSpan.FromHours(newTime);
                var minutes = timeSpan.Minutes < 10 ? string.Format("0" + timeSpan.Minutes) : timeSpan.Minutes.ToString();
                timeText.text = timeText.text = string.Format(timeSpan.Hours + ":" + minutes + " am");
            }
            else if (time >= 12)
            {
                if (time >= 13)
                {
                    var newTime = time - 12;
                    TimeSpan timeSpan = TimeSpan.FromHours(newTime);
                    var minutes = timeSpan.Minutes < 10 ? string.Format("0" + timeSpan.Minutes) : timeSpan.Minutes.ToString();
                    timeText.text = timeText.text = string.Format(timeSpan.Hours + ":" + minutes + " pm");
                }
                else
                {
                    TimeSpan timeSpan = TimeSpan.FromHours(time);
                    var minutes = timeSpan.Minutes < 10 ? string.Format("0" + timeSpan.Minutes) : timeSpan.Minutes.ToString();
                    timeText.text = timeText.text = string.Format(timeSpan.Hours + ":" + minutes + " pm");
                }
            }
            else
            {
                TimeSpan timeSpan = TimeSpan.FromHours(time);
                var minutes = timeSpan.Minutes < 10 ? string.Format("0" + timeSpan.Minutes) : timeSpan.Minutes.ToString();
                timeText.text = timeText.text = string.Format(timeSpan.Hours + ":" + minutes + " am");
            }
        }
    }
}