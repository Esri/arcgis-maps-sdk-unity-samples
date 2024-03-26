using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.GameEngine.Geometry;
using Esri.GameEngine.Layers;
using Esri.GameEngine.Layers.BuildingScene;
using Esri.Unity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocationCycle : MonoBehaviour
{
    public enum Locations
    {
        ChristChurch,
        Esri,
        Girona,
        MountEverest,
        NewYorkCity,
        SanFransisco
    }

    private ArcGISTabletopControllerComponent tableTopController;
    private ArcGISMapComponent arcGISMapComponent;
    [SerializeField] private Locations locations;
    private Locations nextLocation;
    private Locations previousLocation;

    [SerializeField] private Image locationImage;
    [SerializeField] private Sprite christChurch;
    [SerializeField] private Sprite redlands;
    [SerializeField] private Sprite girona;
    [SerializeField] private Sprite mtEverest;
    [SerializeField] private Sprite NYC;
    [SerializeField] private Sprite sanFran;

    private ArcGISBuildingSceneLayer christChurchLayer;
    private ArcGIS3DObjectSceneLayer christChurchSurroundingsLayer;
    private ArcGISBuildingSceneLayer esriLayer;
    private ArcGIS3DObjectSceneLayer esriSurroundings;
    private ArcGISIntegratedMeshLayer gironaLayer;
    private ArcGIS3DObjectSceneLayer newYorkBuildings;
    private ArcGIS3DObjectSceneLayer sfBuildings;

    private void Awake()
    {
        tableTopController = GetComponent<ArcGISTabletopControllerComponent>();
        arcGISMapComponent = GetComponentInChildren<ArcGISMapComponent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        christChurchLayer = new ArcGISBuildingSceneLayer(
            "https://tiles.arcgis.com/tiles/pmcEyn9tLWCoX7Dm/arcgis/rest/services/cclibrary1_wgs84/SceneServer",
            "ChristChurch", 1.0f, false, "");
        christChurchSurroundingsLayer = new ArcGIS3DObjectSceneLayer("https://tiles.arcgis.com/tiles/pmcEyn9tLWCoX7Dm/arcgis/rest/services/ccbuildings_wgs84/SceneServer",
            "CCBuildings", 1.0f, false, "");
        esriLayer = new ArcGISBuildingSceneLayer(
            "https://tiles.arcgis.com/tiles/V6ZHFr6zdgNZuVG0/arcgis/rest/services/Bldg_E_Color_UC2020_demo/SceneServer",
            "Esri Building E", 1.0f, false, "");
        esriSurroundings = new ArcGIS3DObjectSceneLayer("https://services.arcgis.com/hAJQfubNy25iblZJ/arcgis/rest/services/HQ_Campus_WSL1/SceneServer",
            "EsriBuildings", 1.0f, false, "");
        gironaLayer = new ArcGISIntegratedMeshLayer(
            "https://tiles.arcgis.com/tiles/z2tnIkrLQ2BRzr6P/arcgis/rest/services/Girona_Spain/SceneServer",
            "Girona", 1.0f, false, "");
        newYorkBuildings = new ArcGIS3DObjectSceneLayer(
            "https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_NewYork_17/SceneServer",
            "NewYork", 1.0f, false, "");
        sfBuildings = new ArcGIS3DObjectSceneLayer(
            "https://tiles.arcgis.com/tiles/z2tnIkrLQ2BRzr6P/arcgis/rest/services/SanFrancisco_Bldgs/SceneServer",
            "SanFran", 1.0f, false, "");
        esriLayer.DoneLoading += ToggleLayers;

        if (christChurch != null)
        {
            arcGISMapComponent.Map.Layers.Add(christChurchLayer);
        }

        if (esriLayer != null)
        {
            arcGISMapComponent.Map.Layers.Add(esriLayer);
        }

        if (gironaLayer != null)
        {
            arcGISMapComponent.Map.Layers.Add(gironaLayer);
        }

        if (newYorkBuildings != null)
        {
            arcGISMapComponent.Map.Layers.Add(newYorkBuildings);
        }

        if (sfBuildings != null)
        {
            arcGISMapComponent.Map.Layers.Add(sfBuildings);
        }

        if(esriSurroundings != null)
        {
            arcGISMapComponent.Map.Layers.Add(esriSurroundings);
        }

        if (christChurchSurroundingsLayer != null)
        {
            arcGISMapComponent.Map.Layers.Add(christChurchSurroundingsLayer);
        }

        SetLocation();
    }

    private void ToggleLayers(Exception loadError)
    {
        var size = esriLayer.Sublayers.GetSize();

        for (ulong i = 0; i < size; i++)
        {

            var sublayer = esriLayer.Sublayers.At(i);

            sublayer.IsVisible = true;
        }
    }

    public void SetChristChurch()
    {
        tableTopController.Width = 2000.0f;
        tableTopController.Center = new ArcGISPoint(19217150.487337273, -5393301.4510809937, 0.0f, ArcGISSpatialReference.WebMercator());
        tableTopController.ElevationOffset = 0.0f;
        nextLocation = Locations.Esri;
        previousLocation = Locations.SanFransisco;
        locationImage.sprite = christChurch;
        christChurchLayer.IsVisible = true;
        christChurchSurroundingsLayer.IsVisible = true;
        esriLayer.IsVisible = false;
        gironaLayer.IsVisible = false;
        newYorkBuildings.IsVisible = false;
        sfBuildings.IsVisible = false;
        esriSurroundings.IsVisible = false;
    }

    public void SetEverest()
    {
        tableTopController.Width = 10000.0f;
        tableTopController.Center = new ArcGISPoint(9676446.737205295, 3247473.554732518, 0.0f, ArcGISSpatialReference.WebMercator());
        tableTopController.Width = 10000.0f;
        tableTopController.ElevationOffset = -5000.0f;
        previousLocation = Locations.Girona;
        nextLocation = Locations.NewYorkCity;
        locationImage.sprite = mtEverest;
        christChurchLayer.IsVisible = false;
        esriLayer.IsVisible = false;
        gironaLayer.IsVisible = false;
        newYorkBuildings.IsVisible = false;
        sfBuildings.IsVisible = false;
        christChurchSurroundingsLayer.IsVisible = false;
        esriSurroundings.IsVisible = false;
    }

    public void SetGirona()
    {
        tableTopController.Width = 2000.0f;
        tableTopController.Center = new ArcGISPoint(314076.81132414174, 5157894.163259039, 0.0f, ArcGISSpatialReference.WebMercator());
        tableTopController.Width = 1000.0f;
        tableTopController.ElevationOffset = -75.0f;
        previousLocation = Locations.Esri;
        nextLocation = Locations.MountEverest;
        locationImage.sprite = girona;
        christChurchLayer.IsVisible = false;
        esriLayer.IsVisible = false;
        gironaLayer.IsVisible = true;
        newYorkBuildings.IsVisible = false;
        sfBuildings.IsVisible = false;
        christChurchSurroundingsLayer.IsVisible = false;
        esriSurroundings.IsVisible = false;
    }

    public void SetNYC()
    {
        tableTopController.Width = 2500.0f;
        tableTopController.Center = new ArcGISPoint(-8238310.235646995, 4970071.5791424215, 0.0f, ArcGISSpatialReference.WebMercator());
        tableTopController.ElevationOffset = 0.0f;
        previousLocation = Locations.MountEverest;
        nextLocation = Locations.SanFransisco;
        locationImage.sprite = NYC;
        christChurchLayer.IsVisible = false;
        esriLayer.IsVisible = false;
        gironaLayer.IsVisible = false;
        newYorkBuildings.IsVisible = true;
        sfBuildings.IsVisible = false;
        christChurchSurroundingsLayer.IsVisible = false;
        esriSurroundings.IsVisible = false;
    }

    public void SetRedlands()
    {
        tableTopController.Width = 2000.0f;
        tableTopController.Center = new ArcGISPoint(-13046568.699492734, 4036484.647920266, 0.0f, ArcGISSpatialReference.WebMercator());
        tableTopController.ElevationOffset = -390.0f;
        nextLocation = Locations.Girona;
        previousLocation = Locations.ChristChurch;
        locationImage.sprite = redlands;
        christChurchLayer.IsVisible = false;
        christChurchSurroundingsLayer.IsVisible = false;
        esriLayer.IsVisible = true;
        esriSurroundings.IsVisible = true;
        gironaLayer.IsVisible = false;
        newYorkBuildings.IsVisible = false;
        sfBuildings.IsVisible = false;
    }

    public void SetSanFran()
    {
        tableTopController.Width = 2500.0f;
        tableTopController.Center = new ArcGISPoint(-13627665.271218061, 4547675.354340553, 0.0f, ArcGISSpatialReference.WebMercator());
        tableTopController.ElevationOffset = 0.0f;
        previousLocation = Locations.NewYorkCity;
        nextLocation = Locations.ChristChurch;
        locationImage.sprite = sanFran;
        christChurchLayer.IsVisible = false;
        esriLayer.IsVisible = false;
        gironaLayer.IsVisible = false;
        newYorkBuildings.IsVisible = false;
        sfBuildings.IsVisible = true;
        christChurchSurroundingsLayer.IsVisible = false;
        esriSurroundings.IsVisible = false;
    }

    public void SetLocation()
    {
        if (locations == Locations.ChristChurch)
        {
            SetChristChurch();
        }
        else if (locations == Locations.Esri)
        {
            SetRedlands();
        }
        else if (locations == Locations.Girona)
        {
            SetGirona();
        }
        else if (locations == Locations.MountEverest)
        {
            SetEverest();
        }
        else if (locations == Locations.NewYorkCity)
        {
            SetNYC();
        }
        else if (locations == Locations.SanFransisco)
        {
            SetSanFran();
        }
    }

    public void NextLocation()
    {
        locations = nextLocation;
        SetLocation();
    }

    public void PreviousLocation()
    {
        locations = previousLocation;
        SetLocation();
    }
}
