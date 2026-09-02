#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class MissingArcGISPackageWarning
{
    private const string SessionKey = "ArcGISMapsSDK_MissingWarningShown";
    private const double ConsoleRepeatSeconds = 15.0;
    private const string SetupReadmeUrl = "https://github.com/Esri/arcgis-maps-sdk-unity-samples/blob/main/sample_project/README.md";
    private static double lastConsoleWarningTime = -ConsoleRepeatSeconds;

    static MissingArcGISPackageWarning()
    {
#if !USE_ARCGIS_MAPS_SDK
        EditorApplication.update += WaitForEditorReady;
#endif
    }

#if !USE_ARCGIS_MAPS_SDK
    private static void WaitForEditorReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        EditorApplication.update -= WaitForEditorReady;

        if (!SessionState.GetBool(SessionKey, false))
        {
            SessionState.SetBool(SessionKey, true);
            EditorApplication.delayCall += ShowDialog;
        }

        EmitConsoleWarning();
        EditorApplication.update += RepeatConsoleWarning;
    }

    private static void ShowDialog()
    {
        EditorUtility.DisplayDialog(
            "ArcGIS Maps SDK Required",
            "This sample project depends on ArcGIS Maps SDK for Unity (com.esri.arcgis-maps-sdk).\n\n" +
            "The package is not installed, so ArcGIS-dependent sample scripts are disabled to prevent compile errors.\n\n" +
            "Please install ArcGIS Maps SDK for Unity from My Assets (Asset Store) or from Esri's tarball download.\n\n" +
            "Setup instructions: " + SetupReadmeUrl,
            "OK");
    }

    private static void RepeatConsoleWarning()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        if (EditorApplication.timeSinceStartup - lastConsoleWarningTime >= ConsoleRepeatSeconds)
        {
            EmitConsoleWarning();
        }
    }

    private static void EmitConsoleWarning()
    {
        lastConsoleWarningTime = EditorApplication.timeSinceStartup;
        Debug.LogWarning(
            "ArcGIS Maps SDK for Unity is not installed. Install com.esri.arcgis-maps-sdk from the Asset Store or Esri tarball before opening ArcGIS sample scenes. Setup instructions: " + SetupReadmeUrl);
    }
#endif
}
#endif
