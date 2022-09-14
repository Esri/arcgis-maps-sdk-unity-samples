# Measure

Draw lines on the map and show distance.

![Measure](Measure.PNG)

## How to use the sample

1. Set your API Key in the ArcGISMapActor using the World Outliner window (if you are using the SampleViewerLevel you can also set the key through the UI or the level blueprint).
2. While holding shift, click on the map to begin measuring. Continue to click on the map to add additional vertices. The accumulated distance will be shown. This sample is only set up to work with a mouse and keyboard.

## How it works

1. This sample uses [ArcGISGeometryEngine](https://developers.arcgis.com/unity/api-reference/gameengine/geometry/arcgisgeometryengine#distancegeodetic) to calculate distance. 
2. The measured distance is [geodetic/geodesic distance](https://pro.arcgis.com/en/pro-app/2.8/tool-reference/spatial-analyst/geodesic-versus-planar-distance.htm). Geodetic distance is calculated in a 3D spherical space as the distance across the curved surface of the world.
3. Interpolation points are optional and are used to align line segments to terrain. 

## Tags

measure, geometry, analysis, raycast
