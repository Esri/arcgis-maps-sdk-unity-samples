// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkyControllerComponent : MonoBehaviour
{
    private enum SkyMode
    {
        None,
        Animated,
        Simulated
    }

    private enum TimeMode
    {
        MilitaryTime,
        AMPM
    }

    private SkyMode skyMode;
    private TimeMode timeMode;

    [Header("UI Elements")]
    [SerializeField] private Toggle animateToggle;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button collapseButton;
    [SerializeField] private Toggle militaryTimeToggle;
    [SerializeField] private Image moonIcon;
    [SerializeField] private TextMeshProUGUI moonText;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Toggle simulateToggle;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button startTimeDecrease;
    [SerializeField] private Button startTimeIncrease;
    [SerializeField] private TextMeshProUGUI startTimeText;
    [SerializeField] private Button stopTimeDecrease;
    [SerializeField] private Button stopTimeIncrease;
    [SerializeField] private TextMeshProUGUI stopTimeText;
    [SerializeField] private Image sunIcon;
    [SerializeField] private TextMeshProUGUI sunText;
    [SerializeField] private GameObject timeRange;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TextMeshProUGUI timeText;

    private double offset = -90;

    [Header("Time Variables")]
    [Range(0f, 0.1f)]
    [SerializeField] private float speed = 0.1f;

    [Range(0f, 24f)]
    [SerializeField] private double startTime = 6.0;

    [Range(0f, 24f)]
    [SerializeField] private double stopTime = 18.0;

    [Range(0f, 24f)]
    [SerializeField] private double sunRise;

    [Range(0f, 24f)]
    [SerializeField] private double sunSet;

    [Range(0f, 24f)]
    [SerializeField] private double time = 6.0;

    private void CalculateTime(double InTime, TextMeshProUGUI Text)
    {
        if (timeMode == TimeMode.MilitaryTime)
        {
            TimeSpan timeSpan = TimeSpan.FromHours(InTime);
            var hours = timeSpan.Hours < 10 ? string.Format("0" + timeSpan.Hours) : timeSpan.Hours.ToString();
            var minutes = timeSpan.Minutes < 10 ? string.Format("0" + timeSpan.Minutes) : timeSpan.Minutes.ToString();
            Text.text = string.Format(hours + ":" + minutes);
        }
        else
        {
            if (InTime <= 0)
            {
                var newTime = InTime + 12;
                TimeSpan timeSpan = TimeSpan.FromHours(newTime);
                var minutes = timeSpan.Minutes < 10 ? string.Format("0" + timeSpan.Minutes) : timeSpan.Minutes.ToString();
                Text.text = Text.text = string.Format(timeSpan.Hours + ":" + minutes + " am");
            }
            else if (InTime >= 12)
            {
                if (InTime >= 13)
                {
                    var newTime = InTime - 12;
                    TimeSpan timeSpan = TimeSpan.FromHours(newTime);
                    var minutes = timeSpan.Minutes < 10 ? string.Format("0" + timeSpan.Minutes) : timeSpan.Minutes.ToString();
                    Text.text = Text.text = string.Format(timeSpan.Hours + ":" + minutes + " pm");
                }
                else
                {
                    TimeSpan timeSpan = TimeSpan.FromHours(InTime);
                    var minutes = timeSpan.Minutes < 10 ? string.Format("0" + timeSpan.Minutes) : timeSpan.Minutes.ToString();
                    Text.text = Text.text = string.Format(timeSpan.Hours + ":" + minutes + " pm");
                }
            }
            else if (InTime >= 0 && InTime < 1)
            {
                var newTime = InTime + 12;
                TimeSpan timeSpan = TimeSpan.FromHours(newTime);
                var minutes = timeSpan.Minutes < 10 ? string.Format("0" + timeSpan.Minutes) : timeSpan.Minutes.ToString();
                Text.text = Text.text = string.Format(timeSpan.Hours + ":" + minutes + " am");
            }
            else
            {
                TimeSpan timeSpan = TimeSpan.FromHours(InTime);
                var minutes = timeSpan.Minutes < 10 ? string.Format("0" + timeSpan.Minutes) : timeSpan.Minutes.ToString();
                Text.text = Text.text = string.Format(timeSpan.Hours + ":" + minutes + " am");
            }
        }

        if (time > sunSet || time < sunRise)
        {
            var lampPosts = FindObjectsOfType<LampPostItem>();

            foreach (var lampPost in lampPosts)
            {
                lampPost.GetComponentInChildren<Light>().enabled = true;
            }
        }
        else
        {
            var lampPosts = FindObjectsOfType<LampPostItem>();

            foreach (var lampPost in lampPosts)
            {
                lampPost.GetComponentInChildren<Light>().enabled = false;
            }
        }
    }
    private double RotateSky()
    {
        return time / 24 * 360 + offset;
    }

    private void Start()
    {
        timeSlider.value = (float)time;
        timeRange.SetActive(animateToggle.isOn);
        backgroundImage.rectTransform.sizeDelta = animateToggle.isOn ? new Vector2(backgroundImage.rectTransform.sizeDelta.x, 500) : new Vector2(backgroundImage.rectTransform.sizeDelta.x, 430);
        backgroundImage.rectTransform.anchoredPosition = animateToggle.isOn ? new Vector2(backgroundImage.rectTransform.anchoredPosition.x, -321.02f) : new Vector2(backgroundImage.rectTransform.anchoredPosition.x, -286.07f);
        CalculateTime(startTime, startTimeText);
        CalculateTime(stopTime, stopTimeText);
        speedSlider.value = speed * 100;
        var speedValue = speed * 100;
        speedText.text = String.Format("{0:0.#}", speedValue);
        transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, (float)RotateSky());

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

            CalculateTime(startTime, startTimeText);
            CalculateTime(stopTime, stopTimeText);
        });

        animateToggle.onValueChanged.AddListener(delegate
        {
            if (animateToggle.isOn)
            {
                backgroundImage.rectTransform.sizeDelta = new Vector2(backgroundImage.rectTransform.sizeDelta.x, 500);
                backgroundImage.rectTransform.anchoredPosition = new Vector2(backgroundImage.rectTransform.anchoredPosition.x, -321.02f);
                timeRange.SetActive(true);
                animateToggle.interactable = false;
                simulateToggle.isOn = false;
                simulateToggle.interactable = true;
            }
        });

        simulateToggle.onValueChanged.AddListener(delegate
        {
            if (simulateToggle.isOn)
            {
                timeRange.SetActive(false);
                backgroundImage.rectTransform.sizeDelta = new Vector2(backgroundImage.rectTransform.sizeDelta.x, 430);
                backgroundImage.rectTransform.anchoredPosition = new Vector2(backgroundImage.rectTransform.anchoredPosition.x, -286.07f);
                simulateToggle.interactable = false;
                animateToggle.isOn = false;
                animateToggle.interactable = true;
            }
        });

        startButton.onClick.AddListener(delegate
        {
            if (animateToggle.isOn && skyMode == SkyMode.None)
            {
                skyMode = SkyMode.Animated;
                time = startTime;
                startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop";
                simulateToggle.interactable = false;
                animateToggle.interactable = false;
                timeSlider.interactable = false;
                moonIcon.color = Color.gray;
                sunIcon.color = Color.gray;
                sunIcon.color = Color.gray;
                sunText.color = Color.gray;
            }
            else if (simulateToggle.isOn && skyMode == SkyMode.None)
            {
                skyMode = SkyMode.Simulated;
                startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop";
                simulateToggle.interactable = false;
                animateToggle.interactable = false;
                timeSlider.interactable = false;
                moonIcon.color = Color.gray;
                sunIcon.color = Color.gray;
                sunIcon.color = Color.gray;
                sunText.color = Color.gray;
            }
            else if (skyMode == SkyMode.Animated)
            {
                timeSlider.value = (float)time;
                simulateToggle.interactable = true;
                startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
                skyMode = SkyMode.None;
                timeSlider.interactable = true;
                moonIcon.color = Color.white;
                moonText.color = Color.white;
                sunIcon.color = Color.white;
                sunText.color = Color.white;
            }
            else if (skyMode == SkyMode.Simulated)
            {
                timeSlider.value = (float)time;
                animateToggle.interactable = true;
                startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
                skyMode = SkyMode.None;
                timeSlider.interactable = true;
                moonIcon.color = Color.white;
                sunIcon.color = Color.white;
                sunIcon.color = Color.white;
                sunText.color = Color.white;
            }
        });

        startTimeIncrease.onClick.AddListener(delegate
        {
            if (startTime < 24)
            {
                startTime += 1.0;
            }

            CalculateTime(startTime, startTimeText);
        });

        startTimeDecrease.onClick.AddListener(delegate
        {
            if (startTime > 0)
            {
                startTime -= 1.0;
            }

            CalculateTime(startTime, startTimeText);
        });

        stopTimeIncrease.onClick.AddListener(delegate
        {
            if (stopTime < 24) 
            {
                stopTime += 1.0;
            }

            CalculateTime(stopTime, stopTimeText);
        });

        stopTimeDecrease.onClick.AddListener(delegate
        {
            if (stopTime > 0)
            {
                stopTime -= 1.0;
            }

            CalculateTime(stopTime, stopTimeText);
        });

        speedSlider.onValueChanged.AddListener(delegate
        {
            speed = speedSlider.value / 100;
            var speedValue = speed * 100;
            speedText.text = String.Format("{0:0.#}", speedValue);
        });

        collapseButton.onClick.AddListener(delegate
        {
            backgroundImage.gameObject.SetActive(false);
            settingsButton.gameObject.SetActive(true);
        });

        settingsButton.onClick.AddListener(delegate
        {
            backgroundImage.gameObject.SetActive(true);
            settingsButton.gameObject.SetActive(false);
        });
    }

    private void Update()
    {
        CalculateTime(time, timeText);

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
                            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
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
}