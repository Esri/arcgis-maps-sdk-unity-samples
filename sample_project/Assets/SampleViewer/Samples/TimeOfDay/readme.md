# Visualize Time of Day

Control the Time of Day manually, simulate it at a given speed, or animate it from one time to another.

![Image of Time of Day Sample](TimeOfDay.png)

## How to Setup (Time of Day Sample Level)

1. Open the **TimeOfDay** level.
2. Click on the **ArcGISMap Gameobject** in the Hierarchy panel.
3. Set your API key under the Authentication section in the Inspector panel.
4. Click Play and change the time of day using the settings panel.

## How to Setup (Sample Viewer)

1. Click Play in Unity Editor.
2. Input your API key under the **API Key Drop-down**.
3. Click the **Sample Drop-Down** and select **Time of Day**.

## How it works

1. Create a new C# script and make an HTTP request to [query a feature layer](https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-.htm). 
2. Create a new C# script
   - Create a function that will store data returned from the Feature Query.
   - Create a function to spawn the Feature gameobjects according to the data received in the query.
   - Attach the [**ArcGIS Location Component**](https://developers.arcgis.com/unreal-engine/maps/location-component/) to the Feature data gameobject.
   - Add point lights to the feature items and set them to **off** by default.
3. Create a C# script and add the following components to it:
   - Directional Light for the sun
   - Directional Light for the moon
   - Sky and Fog Settings
4. Calculate the time of day based on the rotation of the Sun/Moon in the scene.
5. Create a canvas for the viewport so users can control the time using a slider or set animation properties.
6. Set a time for the Point Lights to turn on and to turn off.

Note: You can use `Debug.Log()` to print log messages in the **Console** window and see if you are gathering the data properly from the feature service.

## About the data

Data for Boston Lamp Posts is fetched from a [Feature Layer](https://services.arcgis.com/V6ZHFr6zdgNZuVG0/ArcGIS/rest/services/Boston_Street_Light_Locations/FeatureServer/0/query?f=geojson&where=1=1&outfields=*)hosted by Esri.
Data for [Esri Global 3D Buildings Layer](https://basemaps3d.arcgis.com/arcgis/rest/services/OpenStreetMap3D_Buildings_v1/SceneServer)
Elevation data is loaded from the [Terrain 3D elevation layer](https://www.arcgis.com/home/item.html?id=7029fb60158543ad845c7e1527af11e4) hosted by Esri.

## Tags

Feature Layer, Data Collection, Time of Day