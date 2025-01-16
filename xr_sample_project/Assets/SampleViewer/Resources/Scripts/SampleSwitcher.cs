// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class SampleSwitcher : MonoBehaviour
{
    [SerializeField] private bool isSampleViewer;

    [SerializeField] private Button homeButton;
    [SerializeField] private Button arTableTopSceneButton;
    [SerializeField] private Button vrSceneButton;
    [SerializeField] private Button vrTableTopSceneButton;


    public void ChangeScene(string NextScene)
    {
        SceneManager.LoadSceneAsync(NextScene);
    }

    private void Start()
    {
        if (!isSampleViewer)
        {
            homeButton.onClick.AddListener(delegate
            {
                ChangeScene("SampleViewer");
            });

            return;
        }

        vrTableTopSceneButton.onClick.AddListener(delegate
        {
            ChangeScene("ARTableTop");
        });

        vrSceneButton.onClick.AddListener(delegate
        {
            ChangeScene("VR-Sample");
        });

        vrTableTopSceneButton.onClick.AddListener(delegate
        {
            ChangeScene("VRTableTop");
        });
    }
}
