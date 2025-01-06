// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.HPFramework;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Esri.ArcGISMapsSDK.Utils;
using TMPro;
using System;

public class SampleSwitcher : MonoBehaviour
{
    [SerializeField] private string apiKey;
    [SerializeField] private TextMeshProUGUI apiKeyInputField;
    [SerializeField] private GameObject notificationMenu;
    [SerializeField] private GameObject warning;

    private Animator animator;
    private string currentSceneName;
    private string nextSceneName;
    private string projectAPIKey;
    private bool isHDRP = true;

    public GameObject MenuVideo;
    public Button ExitButton;
    public Camera cam;
    public Button[] sceneButtons;
    public Button HDRPButton;
    public Button URPButton;

    private void Start()
    {
        animator = notificationMenu.GetComponent<Animator>();
        cam.enabled = true;

        Invoke("SlideNotification", 2.0f);

        if (!string.IsNullOrEmpty(apiKey))
        {
            CheckAPIKey(apiKey);
        }
        else
        {
            projectAPIKey = ArcGISProjectSettingsAsset.Instance.APIKey;
            CheckAPIKey(projectAPIKey);
        }

        ExitButton.onClick.AddListener(delegate
        {
            DoExitGame();
        });

        SceneManager.sceneLoaded += (s, e) =>
        {
            ApplyApiKey();
        };

#if (UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
        isHDRP = false;
#endif
        SetPipeline();
    }

    private void ApplyApiKey()
    {
        // API Script handles api key differently than the mapcomponent
        var api = FindObjectOfType<APIMapCreator>();
        if (api != null)
        {
            if (api.APIKey == "")
            {
                api.APIKey = apiKey;
            }
            return;
        }

        var mapComponent = FindObjectOfType<ArcGISMapComponent>();
        if (mapComponent != null && mapComponent.APIKey == "")
        {
            mapComponent.APIKey = apiKey;
        }
    }

    private void AddScene()
    {
        currentSceneName = nextSceneName;

        //The scene must also be added to the build settings list of scenes
        SceneManager.LoadSceneAsync(currentSceneName, new LoadSceneParameters(LoadSceneMode.Additive));
    }

    //The ArcGISMapView object gets instantiated in our scenes and that results in the object living in the SampleViewer scene,
    //not the scene we loaded. To work around this we need to remove it before loading the next scene
    private void RemoveArcGISMapView()
    {
        var activeScene = SceneManager.GetActiveScene();
        var rootGOs = activeScene.GetRootGameObjects();
        foreach (var rootGO in rootGOs)
        {
            var hpRoot = rootGO.GetComponent<HPRoot>();
            if (hpRoot != null)
            {
                Destroy(rootGO);
            }
        }
    }

    // Delay pop-up notification
    private void SlideNotification()
    {
        //Play notification menu animation.
        animator.Play("NotificationAnim");
    }

    private void EnableDisableSceneButtons(bool enable)
    {
        foreach (Button btn in sceneButtons)
        {
            btn.interactable = enable;

#if !USE_HDRP_PACKAGE || UNITY_ANDROID || UNITY_IOS
            if (btn.gameObject.GetComponent<DisableSampleButtonsForURP>() || btn.gameObject.GetComponent<DisableSampleButtonForMobile>())
            {
                btn.interactable = false;
            }
#endif
        }
    }

    //Exits the Sample Viewer App
    private void DoExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void CheckAPIKey(string value)
    {
        apiKey = value;
        var keyEmpty = string.IsNullOrEmpty(apiKey);
        apiKeyInputField.text = keyEmpty ? "Enter your API key here..." : apiKey;
        warning?.gameObject?.SetActive(keyEmpty);
        EnableDisableSceneButtons(!keyEmpty);
    }

    public void SetNextSceneName(string text)
    {
        nextSceneName = text;
    }

    // Switch scenes with button click
    private void LoadNextScene()
    {
        var sceneLoadedCount = SceneManager.sceneCount;

        // If no async scene is running, then just load an async scene
        if (sceneLoadedCount == 1)
        {
            AddScene();
        }
        else if (sceneLoadedCount == 2)
        {
            // Change scene
            var doneUnloadingOperation = SceneManager.UnloadSceneAsync(currentSceneName);
            doneUnloadingOperation.completed += (AsyncOperation Operation) =>
            {
                RemoveArcGISMapView();

                AddScene();
            };
        }
    }

    // Keep scene buttons pressed after selection
    public void OnSceneButtonClicked(Button clickedButton)
    {
        int btnIndex = Array.IndexOf(sceneButtons, clickedButton);

        if (btnIndex == -1)
        {
            return;
        }

        EnableDisableSceneButtons(true);

        clickedButton.interactable = false;

        HDRPButton.gameObject.SetActive(true);
        URPButton.gameObject.SetActive(true);

        if (clickedButton.gameObject.GetComponent<DisableSampleButtonsForURP>())
        {
            URPButton.gameObject.SetActive(false);
            isHDRP = true;
            SetPipeline();
        }

        MenuVideo.GetComponent<UnityEngine.Video.VideoPlayer>().Stop();
        MenuVideo.gameObject.SetActive(false);

        cam.enabled = false;

        animator.Play("NotificationAnim_Close");

        LoadNextScene();
    }

    #region RenderingPipeline

    // Keep pipeline buttons pressed after selection
    public void OnPipelineButtonClicked(Button clickedButton)
    {
        isHDRP = clickedButton.Equals(HDRPButton);
        StartCoroutine(PipelineChanged());
    }

    private IEnumerator PipelineChanged()
    {
        var sky = FindObjectOfType<ArcGISSkyRepositionComponent>();
        if (sky != null)
        {
            DestroyImmediate(sky.gameObject);
        }

        yield return null;

        SetPipeline();

        LoadNextScene();

        EnableDisablePipelineButtons();
    }

    private void SetPipeline()
    {
        var pipelineString = isHDRP ? "HDRP" : "URP";
        var assetPath = $"SampleGraphicSettings/Sample{pipelineString}ipeline";
        RenderPipelineAsset pipeline = Resources.Load<RenderPipelineAsset>(assetPath);
        GraphicsSettings.renderPipelineAsset = pipeline;
    }

    private void EnableDisablePipelineButtons()
    {
        HDRPButton.interactable = !isHDRP;
        URPButton.interactable = isHDRP;
    }

    #endregion RenderingPipeline
}