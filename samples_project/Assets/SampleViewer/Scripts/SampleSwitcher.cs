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
    public Dropdown PipelineTypeDropdown;
    public Dropdown SceneDropdown;
    public List<string> SceneList = new List<string>();
    private string PipelineType;
    private string SceneName;

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
#if !(UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
            SetPipeline(PipelineTypeDropdown.options[PipelineTypeDropdown.value].text);
#else
            PipelineTypeDropdown.gameObject.SetActive(false);
            SetPipeline("URP");
#endif
        }

        PopulateSampleSceneList();
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