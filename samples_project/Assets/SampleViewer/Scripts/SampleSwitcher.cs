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

public class SampleSwitcher : MonoBehaviour
{
    public GameObject MenuVideo;
    public Button ExitButton;
    public Camera cam;
    public Button[] sceneButtons;
    public Button[] pipelineButtons;
    private string APIKey;
    private string pipelineModeText;
    private string nextSceneName;
    private string PipelineType;
    private string currentSceneName;
    private int sceneLoadedCount;
    private Animator animator;

    private void Start()
    {
        animator = GameObject.Find("NotificationMenu").GetComponent<Animator>();

        cam.enabled = true;

        Invoke("SlideNotification", 2.0f);

        DisableSceneButtons();

        ExitButton.onClick.AddListener(delegate
        {
            doExitGame();
        });

#if (UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
        SetPipeline("URP");
        SetPipelineColor(1, pipelineButtons[1].colors.selectedColor, false);
#else 
        SetPipeline("HDRP");
        SetPipelineColor(0, pipelineButtons[0].colors.selectedColor, false);
#endif
    }

    private void Update()
    {
        // API Script handles api key differently than the mapcomponent
        var api = FindObjectOfType<SampleAPIMapCreator>();
        if (api != null)
        {
            if (api.APIKey == "")
            {
                api.APIKey = APIKey;
            }
            return;
        }
        
        var mapComponent = FindObjectOfType<ArcGISMapComponent>();
        if (mapComponent != null && mapComponent.APIKey == "")
        {
            mapComponent.APIKey = APIKey;
        }

        sceneLoadedCount = SceneManager.sceneCount;

    }

    private void OnEnable()
    {
        if (APIKey == "")
        {
            Debug.LogError("Set an API Key on the SampleSwitcher Game Object for the samples to function.\nThe README.MD of this repo provides more information on API Keys.");
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
        var ActiveScene = SceneManager.GetActiveScene();
        var RootGOs = ActiveScene.GetRootGameObjects();
        foreach (var RootGO in RootGOs)
        {
            var HP = RootGO.GetComponent<HPRoot>();
            if (HP != null)
            {
                Destroy(RootGO);
            }
        }
    }

    private void StopVideo()
    {
        MenuVideo.GetComponent<UnityEngine.Video.VideoPlayer>().Stop();
        MenuVideo.gameObject.SetActive(false);
    }

    // pipelineType must be HDRP or URP
    private void SetPipeline(string pipelineType)
    {
        PipelineType = pipelineType;
        RenderPipelineAsset pipeline = Resources.Load<RenderPipelineAsset>("SampleGraphicSettings/Sample" + PipelineType + "ipeline");
        GraphicsSettings.renderPipelineAsset = pipeline;
    }

    private IEnumerator PipelineChanged()
    {
        var Sky = FindObjectOfType<ArcGISSkyRepositionComponent>();
        if (Sky != null)
        {
            DestroyImmediate(Sky.gameObject);
        }

        yield return null;

        // Set the Pipeline based on what Pipeline button is clicked
        SetPipeline(pipelineModeText);

        SceneButtonOnClick();
    }

    // Delay pop-up notification
    private void SlideNotification()
    {
        //Play notification menu animation.
        animator.Play("NotificationAnim");
    }

    private void EnableSceneButtons()
    {
        foreach (Button btn in sceneButtons)
        {
            btn.interactable = true;
        }
    }

    private void DisableSceneButtons()
    {
        foreach (Button btn in sceneButtons)
        {
            btn.interactable = false;
        }
    }

    //Exits the Sample Viewer App
    private void doExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ReadStringInput(string apiKey)
    {
        APIKey = apiKey;

        if (APIKey.Length == 100)
        {
            EnableSceneButtons();
        }
        else
        {
            DisableSceneButtons();
        }
    }

    public void SetPipelineText(string text)
    {
        pipelineModeText = text;
    }

    public void SetNextSceneName(string text)
    {
        nextSceneName = text;
    }

    // Switch scenes with button click
    public void SceneButtonOnClick()
    {   
        StopVideo();

        cam.enabled = false;

        animator.Play("NotificationAnim_Close");

        // If no async scene is running, then just load an async scene
        if (sceneLoadedCount == 1)
        {
            AddScene();
        }
        else if (sceneLoadedCount == 2)
        {
            // Change scene
            var DoneUnloadingOperation = SceneManager.UnloadSceneAsync(currentSceneName);
            DoneUnloadingOperation.completed += (AsyncOperation Operation) =>
            {
                RemoveArcGISMapView();

                AddScene();
            };
        }
    }

    public void PipelineButtonOnClick()
    {
        StartCoroutine(PipelineChanged());
    }

    // Keep scene buttons pressed after selection
    public void OnSceneButtonClicked(Button clickedButton)
    {
        int btnIndex = System.Array.IndexOf(sceneButtons, clickedButton);

        if (btnIndex == -1)
        {
            return;
        }

        EnableSceneButtons();

        clickedButton.interactable = false;
    }

    // Keep pipeline buttons pressed after selection
    public void OnPipelineButtonClicked(Button clickedButton)
    {
        int btnIndex = System.Array.IndexOf(pipelineButtons, clickedButton);

        if (btnIndex == -1)
        {
            return;
        }

        foreach (Button btn in pipelineButtons)
        {
            btn.interactable = true;
        }

        clickedButton.interactable = false;
    }

    // Set HDRP/URP button color
    public void SetPipelineColor(int index, Color color, bool active)
    {
        var colors = pipelineButtons[index].colors;
        colors.normalColor = color;
        pipelineButtons[index].colors = colors;
        pipelineButtons[index].interactable = active;
    }

    // Unload HDRP/URP button color
    public void UnloadPipelineColor(int index, Color color, bool active)
    {
        var colors = pipelineButtons[index].colors;
        colors.normalColor = new Color(0.498f, 0.459f, 0.588f, 1.0f);
        pipelineButtons[index].colors = colors;
        pipelineButtons[index].interactable = active;
    }

    // Unload HDRP button color
    public void UnloadHDRPColor()
    {
        UnloadPipelineColor(0, pipelineButtons[0].colors.normalColor, true);
    }

    // Unload URP button color
    public void UnloadURPColor()
    {
        UnloadPipelineColor(1, pipelineButtons[1].colors.normalColor, true);
    }
}