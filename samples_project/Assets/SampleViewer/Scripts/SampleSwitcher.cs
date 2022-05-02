// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.HPFramework;
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
            PipelineChanged();
        });
        PipelineType = PipelineTypeDropdown.options[PipelineTypeDropdown.value].text;

        PopulateSampleSceneList();
    }

    private void PopulateSampleSceneList()
    {
        SceneDropdown.options.Clear();

        // Make a list of the formal names of the samples.
        var ApplicationPath = Application.dataPath;
        var SamplePath = ApplicationPath + "/SampleViewer/Samples/";
        List<string> SceneList = new List<string>();
        if (Directory.Exists(SamplePath))
        {
            var SampleScenePaths = Directory.EnumerateFiles(SamplePath, "*.unity", SearchOption.AllDirectories);
            foreach (string CurrentFile in SampleScenePaths)
            {
                string SceneName = Path.GetFileNameWithoutExtension(CurrentFile);
                SceneList.Add(SceneName);
            }
        }

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

    private void PipelineChanged()
    {
        PipelineType = PipelineTypeDropdown.options[PipelineTypeDropdown.value].text;
        RenderPipelineAsset pipeline = Resources.Load<RenderPipelineAsset>("SampleGraphicSettings/Sample" + PipelineType + "ipeline");
        GraphicsSettings.renderPipelineAsset = pipeline;

        SceneChanged();
    }
}