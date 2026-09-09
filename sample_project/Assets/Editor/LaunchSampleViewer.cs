using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class LaunchSampleViewer
{
    private const string TargetScenePath = "Assets/SampleViewer/SampleViewer.unity";

    static LaunchSampleViewer()
    {
        EditorApplication.delayCall += OnEditorLaunch;
    }

    private static void OnEditorLaunch()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (EditorSceneManager.GetActiveScene().path != TargetScenePath)
            {
                Debug.Log($"[ForceStartScene] Loading default launch scene: {TargetScenePath}");
                EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
            }
        }
    }
}
