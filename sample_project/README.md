# arcgis-maps-sdk-unity-samples

![image](arcgis-maps-sdk-unity-samples.png)

Here is a Unity project containing a set of samples showing you how to accomplish various things using the combined features of Unity and the ArcGIS Maps SDK for Unity. The `main` branch is configured to work with our most recent release. If you want to use the sample repo with an older release, check out the corresponding tag of the sample repo, `git checkout 1.0.0` for the sample repo that worked with our 1.0.0 SDK release.

### Note

This repository is made up of two separate Unity projects. If you would like to see the samples made for regular use such as Feature Layer and Routing, please use and set up the **sample_project**. If you are interested in XR Samples such as the Virtual Reality Sample and the XR version of our Table Top Sample, please use and set up the **sample_xr**. Both projects may be used and set up simultaneously, but they do not contain the same samples.

### Requirements for Sample Project

* Computer running Windows or macOS
* The Unity project requires a minimum of Unity `2022.3.62f2`
* ArcGISMaps SDK for Unity

## Instructions

### Prerequisite: 
1. please follow the instructions [here](https://github.com/Esri/arcgis-maps-sdk-unity-samples/blob/main/README.md#instructions) for cloning the project and adding it to Unity
2. Refer to the [ArcGIS Maps SDK for Unity's documentation on getting started](https://developers.arcgis.com/unity/get-started/) on how to download `Unity` and the `ArcGIS Maps SDK for Unity`.

### Setting up the Project 

1. Open sample_project in Unity ignoring the errors when prompted to enter `Safe Mode`.

2. There are two options for adding the plugin to the project: 
   - Assuming you downloaded the plugin from the developer site, use the package manager to import the `.tarball` downloaded in step 2.

      ![image](../package-manager.png)

   - An alternative method to adding the plugin to the project is by using the [Unity Asset Store](https://assetstore.unity.com/packages/tools/integration/arcgis-maps-sdk-for-unity-258537). Login with your Unity account and add it to your assets. Then within Unity, go to the package manager and find it under 'My Assets' and click install.

3. Import the samples. These samples include some components necessary for this repo to function including the `ArcGIS Camera Controller` component.

4. Launch Unity and open the `SampleViewer` level (it should open by default).

5. In the hierarchy select the `SampleSwitcher` Game Object and then in the inspector set your [API key](https://developers.arcgis.com/documentation/security-and-authentication/api-key-authentication/). For the detailed steps to create an API key, see [Create and manage an API key tutorials](https://developers.arcgis.com/documentation/security-and-authentication/api-key-authentication/tutorials/create-an-api-key/) in the _Security and authentication guide_.

6. (Optional) If you want to be able to open the `.cs` files in this project and have IntelliSense recognize variable correctly, in Unity navigate to `Edit -> Preferences -> External Tools -> Generate .csproj files for 'local tarball`

## Licensing

Copyright 2022 - 2025 Esri.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt]( https://raw.github.com/Esri/arcgis-maps-sdk-unity-samples/master/license.txt) file.
