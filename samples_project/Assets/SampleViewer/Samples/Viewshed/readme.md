# Viewshed

This sample project uses a shader-based approach to demonstrate a viewshed overlay effect showing an observer camera's viewable area. Controls can be used to adjust the observer camera and effect parameters.

## How it works

1. Using the `DepthToColorShader`, the observer camera's depth buffer is rendered to a `RenderTexture`.
2. The observer's depth buffer is used by the `ViewshedOverlayShader` to determine which fragments lie within view of the observer.
3. Fragments in view are tinted green; Fragments within the observer camera's frustum--but not visible to the observer--are tinted red.
4. Fragments outside the observer camera's frustum are not tinted.
5. As written, the effect will be applied to all opaque geometry within the scene.

## Known Issues
WIP - This sample is a work in progress, not a final version. It may contain bugs beyond the known issues listed below. Source code is likely to change before final release.

1. The effect does not display correctly in HDRP due to shader issues. For now, please use the URP settings when viewing this sample for the best experience.

## About the data

Building models for New York are loaded from a [3D object scene layer](https://tiles.arcgis.com/tiles/z2tnIkrLQ2BRzr6P/arcgis/rest/services/New_York_LoD2_3D_Buildings/SceneServer/layers/0) hosted by Esri.

Elevation data is loaded from the [Terrain 3D elevation layer](https://www.arcgis.com/home/item.html?id=7029fb60158543ad845c7e1527af11e4) hosted by Esri.

## Tags

viewshed, visibility, visibility analysis
