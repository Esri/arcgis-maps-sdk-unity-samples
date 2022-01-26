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
	public GameObject HandUI;
	public bool HandtUIActive = true;
	public Dropdown SceneDropdown;
	private string SceneName;
	Dictionary<string,int> SceneMapping = new Dictionary<string, int>();


	void Start()
	{
		DisplayHandUI();
		PopulateSampleSceneList();
		SceneDropdown.onValueChanged.AddListener(delegate {
				SceneChanged();
		});
		
		//create initial scene
		if (SceneManager.GetActiveScene().name == "SampleStart")
        {
			AddScene();
		}
		UpdateDropDownSelection();		
	}

	private void SceneChanged()
	{
		string CurrentSelection = SceneDropdown.options[SceneDropdown.value].text;
		string CurrentScene = SceneManager.GetActiveScene().name;
		if (CurrentScene != CurrentSelection)
			AddScene();
	}
	private void UpdateDropDownSelection()
    {
		string CurrentSelection = SceneDropdown.options[SceneDropdown.value].text;
		string CurrentScene=SceneManager.GetActiveScene().name;
		if(CurrentScene !=CurrentSelection)	
		{
			SceneDropdown.value = SceneMapping[CurrentScene];
		}	
	}
	private void AddScene()
	{
		SceneName = SceneDropdown.options[SceneDropdown.value].text;
		
		SceneManager.LoadScene(SceneName);
		//The scene must also be added to the build settings list of scenes		
	}

	public void MenuPressed(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			DisplayHandUI();
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
		int n = 0;
		if (Directory.Exists(SamplePath))
		{
			var Scenes = Directory.EnumerateFiles(SamplePath, "*.unity");
			foreach (string CurrentFile in Scenes)
			{
				string FileName = CurrentFile.Substring(SamplePath.Length, CurrentFile.Length - (".unity").Length - SamplePath.Length);
				SceneList.Add(FileName);
				SceneMapping[FileName] = n;
				n++;
			}
		}
		SceneDropdown.AddOptions(SceneList);
	}


	public void DisplayHandUI()
	{
		if (HandtUIActive)
		{
			HandUI.SetActive(false);
			HandtUIActive = false;
		}
		else
		{
			HandUI.SetActive(true);
			HandtUIActive = true;
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
