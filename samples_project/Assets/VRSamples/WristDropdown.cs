using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Esri.ArcGISMapsSDK.Components;
using Unity.XR.CoreUtils;

public class WristDropdown : MonoBehaviour
{
	// Start is called before the first frame update
	public GameObject wristUI;
	public bool activeWristUI = true;
	public Dropdown SceneDropdown;
	private string SceneName;
	private XROrigin XRRig;

    void Start()
	{
		DisplayWristUI();
		
		SceneDropdown.onValueChanged.AddListener(delegate {
			SceneChanged();
		});
		PopulateSampleSceneList();
	}

	private void SceneChanged()
	{
		AddScene();
	}
	private void AddScene()
	{
		SceneName = SceneDropdown.options[SceneDropdown.value].text;
		//The scene must also be added to the build settings list of scenes
		SceneManager.LoadScene(SceneName);
	}

	public void MenuPressed(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			DisplayWristUI();
		}
	}
	public void ExitGame()
	{
		Debug.Log("quit");
		Application.Quit();
	}
	private void PopulateSampleSceneList()
	{
		SceneDropdown.options.Clear();
		var ApplicationPath = Application.dataPath;
		var SamplePath = ApplicationPath + "/VRSamples/Resources/SampleScenes/";
		List<string> SceneList = new List<string>();
		if (Directory.Exists(SamplePath))
		{
			var Scenes = Directory.EnumerateFiles(SamplePath, "*.unity");
			foreach (string CurrentFile in Scenes)
			{
				string FileName = CurrentFile.Substring(SamplePath.Length, CurrentFile.Length - (".unity").Length - SamplePath.Length);
				SceneList.Add(FileName);
			}
		}
		SceneDropdown.AddOptions(SceneList);
		//AddScene();
	}

	/*
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
	*/


	public void DisplayWristUI()
	{
		if (activeWristUI)
		{
			wristUI.SetActive(false);
			activeWristUI = false;
		}
		else
		{
			wristUI.SetActive(true);
			activeWristUI = true;
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
