// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

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

#if !(UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
        PipelineType = PipelineTypeDropdown.options[PipelineTypeDropdown.value].text;
#else
        PipelineType = "URP";
        PipelineTypeDropdown.gameObject.SetActive(false);
        RenderPipelineAsset pipeline = Resources.Load<RenderPipelineAsset>("SampleGraphicSettings/Sample" + PipelineType + "ipeline");
        GraphicsSettings.renderPipelineAsset = pipeline;
#endif
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

    private IEnumerator PipelineChanged()
    {
        var Sky = FindObjectOfType<ArcGISSkyRepositionComponent>();
        if (Sky != null)
        {
            DestroyImmediate(Sky.gameObject);
        }

        yield return null;

        PipelineType = PipelineTypeDropdown.options[PipelineTypeDropdown.value].text;
        RenderPipelineAsset pipeline = Resources.Load<RenderPipelineAsset>("SampleGraphicSettings/Sample" + PipelineType + "ipeline");
        GraphicsSettings.renderPipelineAsset = pipeline;

        SceneChanged();
    }
}