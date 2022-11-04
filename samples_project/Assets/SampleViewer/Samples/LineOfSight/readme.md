# Check line of sight

Show a line of sight between two objects. Check if the line of sight is obstructed by the ArcGIS 3D object scene layer.

![Image of line of sight](LineOfSight.jpg)

## How to use the sample

1. Open **LineOfSight** scene.
2. Click on **LineOfSightMap** game object in the **Hierarchy** window.
3. Set your API key in the **Inspector** window.
4. Click play and see the line colors changes to red if there is any object to obstruct the sight.

## How it works

1. Have an ArcGIS Map with the [mesh colliders](https://developers.arcgis.com/unity/maps/mesh-collider/) enabled.
2. Attach an [**ArcGIS Camera**](https://developers.arcgis.com/unity/maps/camera/#arcgis-camera) component to the active camera.
3. Make a parent game object with the [**ArcGIS Location**](https://developers.arcgis.com/unity/maps/location-component/) component attached for other game object to be nested.
4. Have a game object that uses the **Transform** component for the target moving object under the game object with the **ArcGIS Location** component.
5. Have a game object for the viewpoint and another for the sight line under the game object with the **ArcGIS Location** component.
6. Have a parent game object and nest the moving object's path points in the area.
7. Have scripts to update the [Transform](https://docs.unity3d.com/ScriptReference/Transform.html) of the moving object according to the path points.
8. Have a script to use [Raycast](https://docs.unity3d.com/ScriptReference/Physics.Raycast.html).
    - Use [`Physics.Raycast`](https://docs.unity3d.com/ScriptReference/Physics.Raycast.html) to check any any obstructions between the viewpoint and the moving object.
    - Use the [`RaycastHit.point`](https://docs.unity3d.com/ScriptReference/RaycastHit-point.html) property to determine where the line of sight from the first object collides.
    - If you have any objects that may interfere with the raycast check, use [`Physics.IgnoreRaycastLayer`](https://docs.unity3d.com/ScriptReference/Physics.IgnoreRaycastLayer.html).

## About the data

Building models for New York are loaded from a [3D object scene layer](https://tiles.arcgis.com/tiles/z2tnIkrLQ2BRzr6P/arcgis/rest/services/New_York_LoD2_3D_Buildings/SceneServer/layers/0) hosted by Esri.

Elevation data is loaded from the [Terrain 3D elevation layer](https://www.arcgis.com/home/item.html?id=7029fb60158543ad845c7e1527af11e4) hosted by Esri.

## Tags

line of sight, raycast, visibility, visibility analysis
