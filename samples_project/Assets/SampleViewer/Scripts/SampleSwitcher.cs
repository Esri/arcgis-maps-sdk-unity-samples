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
    public List<string> SceneList = new List<string>();
    private string PipelineType;
    private string SceneName;
    private bool EnablePipelineSwitching = false;

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
    }

    private void Start()
    {
        SceneDropdown.onValueChanged.AddListener(delegate
        {
            SceneChanged();
        });

        PipelineTypeDropdown.onValueChanged.AddListener(delegate
        {
            StartCoroutine(PipelineChanged());
        });

#if USE_HDRP_PACKAGE
            PipelineTypeDropdown.options.Add(new Dropdown.OptionData("HDRP"));
#endif

#if USE_URP_PACKAGE
            PipelineTypeDropdown.options.Add(new Dropdown.OptionData("URP"));

            Debug.LogError("There is a bug where this project does not work with URP, please remove it until this is resolved");
            return;
#endif

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
            Debug.LogError("This project is configured to only work with eaither the HDRP or URP but not both.\nPlease remove one for this to function");

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
}