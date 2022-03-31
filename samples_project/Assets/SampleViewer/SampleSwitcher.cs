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
    public GameObject LightingObject;
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
        var ApplicationPath = Application.dataPath;
        var SamplePath = ApplicationPath + "/SampleViewer/Samples/";
        List<string> SceneList = new List<string>();
        if (Directory.Exists(SamplePath))
        {
            var Scenes = Directory.EnumerateFiles(SamplePath, "*.unity", SearchOption.AllDirectories);
            foreach (string CurrentFile in Scenes)
            {
                string FileName = Path.GetFileNameWithoutExtension(CurrentFile);
                SceneList.Add(FileName);
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

        if (PipelineType == "HDRP")
        {
            LightingObject.SetActive(true);
        }
        else
        {
            LightingObject.SetActive(false);
        }

        SceneChanged();
    }
}