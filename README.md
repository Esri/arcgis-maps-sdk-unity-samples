# arcgis-maps-sdk-unity-samples

![image](arcgis-maps-sdk-unity-samples.png)

Here is a Unity project containing a set of samples showing you how to accomplish various things using the combined features of Unity and the ArcGIS Maps SDK for Unity. The `main` branch is configured to work with our most recent release. If you want to use the sample repo with an older release, check out the corresponding tag of the sample repo, `git checkout 1.0.0` for the sample repo that worked with our 1.0.0 SDK release.

### Note

This repository is made up of two separate Unity projects. If you would like to see the samples made for regular use such as Feature Layer and Routing, please use and set up the [**sample_project**](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/readme.md). If you are interested in XR Samples such as the Virtual Reality Sample and the XR version of our Table Top Sample, please use and set up the [**sample_xr**](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/xr_sample_project/readme.md). Both projects may be used and set up simultaneously, but they do not contain the same samples.

### Requirements for Sample Project

* Computer running Windows or macOS
* The Unity project requires a minimum of Unity `2022.3.62f2`
* ArcGISMaps SDK for Unity

### Requirements for XR Sample Project

* Computer running Windows (OpenXR is not supported on macOS)
* The Unity project requires a minimum of Unity `6000.3.0f1`
* ArcGISMaps SDK for Unity
* A VR Headset and the necessary software to run through Desktop Mode

## Features
* [Building Filter](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/BuildingFilter/readme.md) - Explore a building scene layer by toggling the visibility of different attributes.
* [Feature service REST API](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/FeatureLayer/readme.md) - See how to query a feature service to create game objects in Unity located at real-world positions.
* [Geocoding](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/Geocoding/readme.md) - Search for an address or click on the surface to get the address of that location.
* [Geometry](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/Geometry/readme.md) - Draw polylines, polygons and envelopes on the map and get their lengths or areas.
* [HitTest](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/HitTest/readme.md) - Visualize individual Buildings ID's from a 3D Object Scene Layer.
* [Identify](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/Identify/readme.md) - Identify individual or multiple buildings feature ID's.
* [Line of sight](https://github.com/Esri/arcgis-maps-sdk-unreal-engine-samples/tree/main/sample_project/Content/SampleViewer/Samples/LineOfSight/readme.md) - See how to check line of sight between two objects in Unity.
* [Material By Attribute](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/MaterialByAttribute/readme.md) - Apply materials to 3DObject Scene layer based on specific attributes.
* [Measure](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/Measure/readme.md) - Click on the map to get real-world distances between points.
* [Overview Map](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/OverviewMap/readme.md) - Use an overview map to better understand where you are in the world.
* [Real Time Weather Query](https://github.com/Esri/arcgis-maps-sdk-unity-samples/blob/main/sample_project/Assets/SampleViewer/Samples/WeatherQuery/readme.md) - Query the current weather in a city using the feature layer query work flow.
* [Routing](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/Routing/readme.md) - See how to query Esri's routing service to get the shortest path between two points and visualize that route in Unity.
* [Stream Layer](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/StreamLayer/readme.md) - See how to use web sockets to connect to an Esri real-time service to update game objects locations in real-time.
* [Third Person Controller](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/ThirdPerson/readme.md) - Explore from the perspective of a third person camera following a controllable character.
* [Time of Day](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/sample_project/Assets/SampleViewer/Samples/TimeOfDay/readme.md) - Visualize and control the time of day in a scene. Use a feature layer for geographically accurate street lamps
* [Viewshed](https://github.com/Esri/arcgis-maps-sdk-unity-samples/blob/main/sample_project/Assets/SampleViewer/Samples/Viewshed/readme.md) - See how to visualize a viewshed effect.

## Features in XR Sample Project

* [ARTableTop](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/xr_sample_project/Assets/SampleViewer/Samples/XRTableTop/readme.md) - See how to configure the camera to visualize a tabletop map on an AR/VR device and control the map with AR/VR controllers and hand tracking.
* [VRSample](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/xr_sample_project//Assets/SampleViewer/Samples/VRSample/readme.md) - See how to configure the camera to visualize content on a VR device and move the camera with VR locomotion.
* [VRTableTop](https://github.com/Esri/arcgis-maps-sdk-unity-samples/tree/main/xr_sample_project/Assets/SampleViewer/Samples/XRTableTop/readme.md) - See how to configure the camera to visualize a tabletop map on a VR device and control the map with VR controllers and hand tracking.

## Instructions

1. Clone this repo.
2. Follow individual instructions for each sample project (sample_project or xr_sample_project) as desired.


## Requirements

* Refer to the [ArcGIS Maps SDK for Unity's documentation on system requirements](https://developers.arcgis.com/unity/system-requirements/)

## Resources

* [ArcGIS Maps SDK for Unity's documentation](https://developers.arcgis.com/unity/)
* [Unity's documentation](https://docs.unity.com/)
* [Esri Community forum](https://community.esri.com/t5/arcgis-maps-sdks-for-unity-questions/bd-p/arcgis-maps-sdks-unity-questions)

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue.

## Contributing

Esri welcomes contributions from anyone and everyone. Please see our [guidelines for contributing](https://github.com/esri/contributing).

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
