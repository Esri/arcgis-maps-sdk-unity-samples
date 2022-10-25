// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.HPFramework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SampleSwitcher : MonoBehaviour
{
    public string APIKey = "";
    public Dropdown PipelineTypeDropdown;
    public Dropdown SceneDropdown;
    public Button ExitButton;
    public List<string> SceneList = new List<string>();
    public List<string> PipelineList = new List<string>();
    private string PipelineType;
    private string SceneName;
    private bool EnablePipelineSwitching = true;

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
#if (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS)
            mapComponent.MapType = Esri.GameEngine.Map.ArcGISMapType.Local;
            mapComponent.EnableExtent = false;
#endif
        }
    }

    private void Start()
    {
        ExitButton.onClick.AddListener(delegate
        {
            doExitGame();
        });
        SceneDropdown.onValueChanged.AddListener(delegate
        {
            SceneChanged();
        });

        PipelineTypeDropdown.onValueChanged.AddListener(delegate
        {
            StartCoroutine(PipelineChanged());
        });

	//Populates Pipeline Dropdown
#if USE_HDRP_PACKAGE
            PipelineList.Add("HDRP");
#endif

#if USE_URP_PACKAGE
            PipelineList.Add("URP");
#endif

        PipelineTypeDropdown.options.Clear();
        PipelineTypeDropdown.AddOptions(PipelineList);

        if (PipelineTypeDropdown.options.Count == 0)
        {
            Debug.LogError("Either HDRP or URP is required for the ArcGIS Maps SDK to work");
            return;
        }
        else if (PipelineTypeDropdown.options.Count == 1)
        {
            SetPipeline(PipelineTypeDropdown.options[PipelineTypeDropdown.value].text);
            PipelineTypeDropdown.gameObject.SetActive(false);
        }
        else
        {

#if !(UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
            SetPipeline(PipelineTypeDropdown.options[PipelineTypeDropdown.value].text);
#else
            PipelineTypeDropdown.gameObject.SetActive(false);
            SetPipeline("URP");
#endif
        }

        if (!EnablePipelineSwitching)
        {
            PipelineTypeDropdown.gameObject.SetActive(false);
        }

        PopulateSampleSceneList();
    }

    private void OnEnable()
    {
        if (APIKey == "")
        {
            Debug.LogError("Set an API Key on the SampleSwitcher Game Object for the samples to function.\nThe README.MD of this repo provides more information on API Keys.");
        }
    }

    private void PopulateSampleSceneList()
    {
        SceneDropdown.options.Clear();

#if !(USE_OPENXR_PACKAGE)
        if (SceneList.Contains("VR-Sample")){ 
             SceneList.Remove("VR-Sample");
        }
#else
        if (!SceneList.Contains("VR-Sample")){ 
             SceneList.Add("VR-Sample");
        }
#endif
        SceneDropdown.AddOptions(SceneList);
        AddScene();
    }

    private void AddScene()
    {
        SceneName = SceneDropdown.options[SceneDropdown.value].text;
        //The scene must also be added to the build settings list of scenes
        SceneManager.LoadSceneAsync(SceneName, new LoadSceneParameters(LoadSceneMode.Additive));   
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

    private void SceneChanged()
    {
        var DoneUnLoadingOperation = SceneManager.UnloadSceneAsync(SceneName);
        DoneUnLoadingOperation.completed += (AsyncOperation Operation) =>
        {
            RemoveArcGISMapView();
            AddScene();
        };
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

        SetPipeline(PipelineTypeDropdown.options[PipelineTypeDropdown.value].text);

        SceneChanged();
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
}