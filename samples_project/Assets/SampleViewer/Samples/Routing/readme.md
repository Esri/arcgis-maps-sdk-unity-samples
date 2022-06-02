# Routing

Show a route between 2 points on a map using Esri's routing service REST API.

![Routing](routing.png)

## How it works

1. Set your API Key in the RouteManager inspector window.
2. While holding shift, left click on the map twice. The route will be shown between the 2 points. This sample is only setup to work with mouse and keyboard.
3. This sample uses Esri's routing service's REST API to query the closest route along the road network between 2 points. This service uses routing operations associated with your API Key. You can learn more about [API Keys here](https://developers.arcgis.com/documentation/mapping-apis-and-services/security/api-keys/).
4. Raycasts are used to determine the elevation at each breadcrumb's position to account for elevation.

## Tags

routing, raycast, REST API
