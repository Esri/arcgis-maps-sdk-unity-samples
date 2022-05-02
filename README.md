# This repo is currently private

# arcgis-maps-sdk-unity-samples

1. Clone this repo and add `samples_project` in the Unity Hub
2. Open the `samples_project` and launch the Unity Editor. If there is a pop-up asking to open the project in Safe Mode, click `Ignore`.
3. Get the most recent build of the plugin from here https://runtime-zip.esri.com/userContent/runtime/setups/gameengine/100.14.0/
4. Install the most recent `ArcGISMapsSDK.unitypackage`. In the header bar navigate to `Assets -> Import Package -> Custom Package` and select `ArcGISMapsSDK.unitypackage`.
5. Open `SampleViewer.unity` from the `Assets/SampleViewer` folder in the Project window.

To add a new sample scene add the `.unity` file in this directory `Assets/SampleViewer/Samples/` and also add the scene to the project settings build list(`File -> Build Settings`).
In addition, to ensure your scene and the `SampleViewer.unity` scene (which is used to preview all scenes) has good lighting add the `LightingManager` prefab to your scene found in `Assets/SampleViewer/Resources/SampleGraphicSettings`.

- [Unity C# Coding Style](coding-style-csharp.md)
- [List of sample ideas](https://esriis.sharepoint.com/:x:/r/teams/GameEngine/_layouts/15/Doc.aspx?sourcedoc=%7B0dcb8b4d-f1ab-406c-9286-8a79ab2f7bc8%7D&action=editnew)

## PR Process

Especially as we begin the process of figuring out how we structure our new samples the more the merrier on your PRs. Also while this is private PRs can be created against main. Testing on as many devices as possible and stating what testing has been done is also a good idea.

## Issue management

If you find a bug with the ArcGIS Maps SDK for Unity or with this sample viewer you can create an issue here. Same goes for a feature request on the sample viewer.
