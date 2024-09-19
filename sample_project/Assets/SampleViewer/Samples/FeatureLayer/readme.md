# Query a feature layer

Query objects from a feature layer and show it on the map.

![Feature layer](FeatureLayer.png)

## How to set up (Feature Layer Level)

1. Open the **FeatureLayer** Scene.
2. Click on the **ArcGISMap** Gameobject in the hierarchy window.
3. Set your API key under Authentication section in the inspector window.
4. Click play.


## How to set up (Sample Viewer)

1. Click Play in Unity Editor.
2. Input your API key under the **API Key Drop down**.
3. Click the **Sample Drop Down** and select **Feature Layer**.

## How to use

1. Already available is a Feature Layer of park trees. Upon running the scene, these feature items will be spawned in at their default latitude and longitude, and with all of the properties associated with that feature item. 
2. Clicking on any of these objects under the **FeatureLayer** Gameobject in Unity to see its latitude, longitude, and any properties received by the query.
3. Under the outfields dropdown, by default it will be set to "Get All Outfields". Selecting any of the other outfields will deselect "Get All Outfields". Multi-select is supported. Outfields are the properties associated with the individual features. **Note**: after modifying the outfields selections, it is necessary to reprocess the feature layer query for the changes to take effect.

## How it works

### Important note

This sample allows users to use their own feature layer data sets to visualize in Unity. At this time, we only support `Point Layer data types`.

### Getting the data

In this sample, the [FeatureService REST API](https://developers.arcgis.com/rest/services-reference/enterprise/layer-feature-service-.htm) is used to perform a query operation. The result of a query operation is a list of features that includes the feature's location as well as some attributes giving more details about the feature. In this sample, the stadium name and team name were used.
The `FeatureLayer.cs` file makes the request and then parses the result to construct the game objects based on the results. The **ArcGIS Location** component can be attached to these new game objects in order to locate them in the world. Use the `FeatureData.cs` script to store information specific to the feature, such as the team name, stadium name, the team's division in the MLB. The team's league is used to determine how the stadium is rendered in the game.

### Making the request

To make the request we append `/Query` onto the end of the Feature layer we want to get data from. Additional request headers are used to control what content we get back. You can learn all about the Query REST API [here](https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.htm). In this example we use a few of them: `f=geojson` to control the output format of the response; `where=1=1` to get all the features back, in some cases especially datasets with a large amount of features using an intelligent where clause can ensure you only request the data that you need; `outSR=4326` makes it so that the geometries in the response are in the WGS84 spatial reference, make sure you parse the geometry in this spatial reference; and finally `outFields=*` this tells the service which attributes to return. The `outFields` are unique from feature layer to feature layer so make sure you know your data and what fields you want to get in the response. Setting `outFields=*` will get all outfields, however if you only want to get a select ones, you can replace the * with those outfields. 

### Parsing the response

After the request succeeds we will parse the text and turn it into meaningful `GameObjects`. Parsing the response can be a bit tedious because there is lots of string parsing to ensure you get the data you need. `CreateGameObjectsFromResponse(string Response)` in `FeatureLayer.cs` shows an example of how this can be accomplished which will differ depending on the request headers you sent. A new `GameObject` is created for each feature and an `ArcGISLocation` component is attached with the values modified to be the feature's location. In this sample, a `FeatureData` component is also attached to the new game object.

### Navigating the scene

These new game objects are then used to populate the drop-down list allowing you to quickly navigate to other stadiums by updating the `ArcGIS Camera` location. When flying around the scene you will see stadiums if you know where to look or floating high above in the sky there are capsules to show you where each stadium is.

## About the data

This sample uses the [park trees](https://services.arcgis.com/V6ZHFr6zdgNZuVG0/ArcGIS/rest/services/ParkTrees/FeatureServer/0/) from Esri's LivingAtlas [portal item](https://www.arcgis.com/home/item.html?id=f60004d3037e42ad93cb03b9590cafec).

## Tags

feature, feature layer, feature service
