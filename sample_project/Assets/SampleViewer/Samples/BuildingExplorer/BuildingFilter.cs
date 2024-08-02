using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.ArcGISMapsSDK.Components;
using Esri.Unity;
using Esri.GameEngine.Layers;
using Esri.GameEngine.Layers.BuildingScene;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using Esri.GameEngine;
public class BuildingFilter : MonoBehaviour
{
    [SerializeField] private Button addNewBSL;
    [SerializeField] private ArcGISLocationComponent cameraLocation;
    [SerializeField] private Toggle disciplineToggle;
    [SerializeField] private TextMeshProUGUI Denom;
    [SerializeField] private TextMeshProUGUI failedToLoadText;
    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private GameObject interfaceObject;
    [SerializeField] private Button leftPhase;
    [SerializeField] private Button leftLevel;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Button rightLevel;
    [SerializeField] private Button rightPhase;
    [SerializeField] private Scrollbar phaseSlider;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TMP_InputField serviceURL;
    [SerializeField] private Button trashButton;
    [SerializeField] private Sprite visibleSprite;

    private ArcGISMapComponent arcGISMapComponent;
    private ArcGISBuildingSceneLayer buildingSceneLayer;
    private ContentBoxes contentBoxes;
    private int levelNumber;
    private int phaseNumber;
    public BuildingStatistics buildingStatistics = new BuildingStatistics();
    public List<Discipline> disciplineCategoryData = new List<Discipline>();


    // Start is called before the first frame update
    void Start()
    {

        InitializeBuildingSceneLayer();

        disciplineToggle.onValueChanged.AddListener(delegate (bool active)
        {
            if (buildingSceneLayer.LoadStatus != ArcGISLoadStatus.NotLoaded)
            {

                disciplineToggle.isOn = active;
                interfaceObject.SetActive(active);
                contentBoxes = FindObjectOfType<ContentBoxes>();
                GetStatistics();
                AddDisciplineCategoryData();
                if (disciplineToggle.isOn)
                {
                    disciplineToggle.GetComponent<Image>().sprite = visibleSprite;
                }
                else
                {
                    disciplineToggle.GetComponent<Image>().sprite = hiddenSprite;
                }
            }




        });
        addNewBSL.onClick.AddListener(delegate
        {
            String URL = serviceURL.text.ToString();

            NewBuildingSceneLayer(URL);

        });
        trashButton.onClick.AddListener(delegate
        {

            levelText.SetText("-");
            if (int.TryParse(phaseText.text, out phaseNumber))
            {
                GenerateWhereClause(buildingStatistics.bldgLevelMax, phaseNumber, true, true);
            }
        });
        leftLevel.onClick.AddListener(delegate
        {

            if (!int.TryParse(levelText.text, out levelNumber))
            {
                // If parsing fails (e.g., because the text is a dash), set levelNumber to max
                levelNumber = buildingStatistics.bldgLevelMax;
                levelText.SetText(levelNumber.ToString());
                if (int.TryParse(phaseText.text, out phaseNumber))
                {
                    GenerateWhereClause(levelNumber, phaseNumber, false, false);
                }

            }

            else if (levelNumber > 0)
            {
                --levelNumber;
                levelText.SetText(levelNumber.ToString());
                if (int.TryParse(phaseText.text, out phaseNumber))
                {
                    GenerateWhereClause(levelNumber, phaseNumber, false, false);
                }
            }
        });

        rightLevel.onClick.AddListener(delegate
        {

            if (!int.TryParse(levelText.text, out levelNumber))
            {
                levelNumber = buildingStatistics.bldgLevelMax;
                levelText.SetText(levelNumber.ToString());
                if (int.TryParse(phaseText.text, out phaseNumber))
                {
                    GenerateWhereClause(levelNumber, phaseNumber, false, false);
                }
            }

            if (levelNumber < buildingStatistics.bldgLevelMax)
            {
                ++levelNumber;
                levelText.SetText(levelNumber.ToString());
                if (int.TryParse(phaseText.text, out phaseNumber))
                {
                    GenerateWhereClause(levelNumber, phaseNumber, false, false);
                }
            }
        });

        phaseSlider.onValueChanged.AddListener(delegate (float value)
        {
            int phaseRange = buildingStatistics.createdPhaseMax - buildingStatistics.createdPhaseMin;
            int phase = Mathf.RoundToInt(value * phaseRange) + buildingStatistics.createdPhaseMin;

            Debug.Log(phase);
            if (int.TryParse(levelText.text, out levelNumber))
            {
                GenerateWhereClause(levelNumber, phase, false, false);
            }
            else
            {
                GenerateWhereClause(buildingStatistics.bldgLevelMax, phase, false, true);
            }

            if (int.TryParse(phaseText.text, out phaseNumber))
            {
                phaseText.SetText(phase.ToString());
            }
        });

        leftPhase.onClick.AddListener(delegate
        {
            phaseSlider.value = Mathf.Clamp(phaseSlider.value - buildingStatistics.stepSize, 0f, 1f);
        });

        rightPhase.onClick.AddListener(delegate
        {
            phaseSlider.value = Mathf.Clamp(phaseSlider.value + buildingStatistics.stepSize, 0f, 1f);
        });
    }
    // Update is called once per frame
    void Update()
    {
    }
    void InitializeBuildingSceneLayer()
    {
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();
        if (arcGISMapComponent != null)
        {
            var allLayers = arcGISMapComponent.Map.Layers;
            for (ulong i = 0; i < allLayers.GetSize(); i++)
            {
                if (allLayers.At(i).GetType().Name == "ArcGISBuildingSceneLayer")
                {
                    Debug.Log(allLayers.At(i).GetType().Name);
                    buildingSceneLayer = allLayers.At(i) as ArcGISBuildingSceneLayer;
                    Debug.Log(buildingSceneLayer.Name);
                }
            }
        }
    }
    void NewBuildingSceneLayer(String source)
    {
        var newLayer = new Esri.GameEngine.Layers.ArcGISBuildingSceneLayer(source, "UserBSL", 1.0f, true, "");
        var solidDef = new ArcGISSolidBuildingFilterDefinition("", "");
        var filter = new ArcGISBuildingAttributeFilter("Filter", "", solidDef);
        arcGISMapComponent.Map.Layers.Add(newLayer);
        Debug.Log("Layer Added");
        StartCoroutine(Delay(5, false, newLayer, solidDef, filter));


    }
    IEnumerator Delay(int time, bool bIsAdded, ArcGISBuildingSceneLayer newLayer, ArcGISSolidBuildingFilterDefinition solidDef, ArcGISBuildingAttributeFilter filter)
    {
        Debug.Log("Delay called");
        loadingText.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);

        if (!bIsAdded)
        {
            Debug.Log("Delay called 1");
            buildingSceneLayer = arcGISMapComponent.Map.Layers.Last() as ArcGISBuildingSceneLayer;
            if (buildingSceneLayer != null)
            {
                var firstLayers = buildingSceneLayer.Sublayers;
                for (ulong i = 0; i < firstLayers.GetSize(); i++)
                {
                    if (firstLayers.At(i).Name == "Full Model")
                    {
                        firstLayers.At(i).IsVisible = true;
                        Debug.Log("first layer set to visible");
                    }
                    else if (firstLayers.At(i).Name == "Overview")
                    {
                        firstLayers.At(i).IsVisible = false;
                        Debug.Log("Overview set to invisible");
                    }
                }
            }
        }

        switch (GetLoadStatus())
        {
            case "Loading":
                StartCoroutine(Delay(time, true, newLayer, solidDef, filter));
                break;

            case "Failed":
                loadingText.gameObject.SetActive(false);
                failedToLoadText.gameObject.SetActive(true);
                yield return new WaitForSeconds(5);
                failedToLoadText.gameObject.SetActive(false);
                break;

            case "Loaded":
                loadingText.gameObject.SetActive(false);
                AddDisciplineCategoryData();
                contentBoxes.RemoveDisciplines();
                contentBoxes.AddDisciplines(disciplineCategoryData);
                GetStatistics();
                filter.SolidFilterDefinition = solidDef;
                buildingSceneLayer.BuildingAttributeFilters.Add(filter);
                buildingSceneLayer.ActiveBuildingAttributeFilter = filter;
                var layerCenter = buildingSceneLayer.Extent.Center;
                arcGISMapComponent.OriginPosition = layerCenter;
                cameraLocation.Position = layerCenter;
                phaseText.text = buildingStatistics.createdPhaseMax.ToString();
                trashButton.onClick.Invoke();
                GenerateWhereClause(buildingStatistics.bldgLevelMax, buildingStatistics.createdPhaseMax, true, true);
                break;
        }

        Debug.Log("adding to layer collection");
    }

    String GetLoadStatus()
    {
        if (buildingSceneLayer == null)
        {
            return "Failed";
        }
        var loadStatus = buildingSceneLayer.LoadStatus;

        if (loadStatus == ArcGISLoadStatus.Loaded)
        {
            return "Loaded";
        }
        else if (loadStatus == ArcGISLoadStatus.Loading)
        {
            return "Loading";
        }
        else
        {
            var index = arcGISMapComponent.Map.Layers.GetSize() - 1;
            arcGISMapComponent.Map.Layers.Remove(index);
            --index;
            buildingSceneLayer = arcGISMapComponent.Map.Layers.At(index) as ArcGISBuildingSceneLayer;
            return "Failed";
        }
    }
    void AddDisciplineCategoryData()
    {
        disciplineCategoryData.Clear();
        if (buildingSceneLayer != null)
        {
            var firstLayers = buildingSceneLayer.Sublayers;
            for (ulong i = 0; i < firstLayers.GetSize(); i++)
            {
                if (firstLayers.At(i).Name == "Full Model")
                {
                    var secondLayers = firstLayers.At(i).Sublayers;
                    for (ulong j = 0; j < secondLayers.GetSize(); j++)
                    {
                        secondLayers.At(j).IsVisible = true;
                        Discipline newDiscipline = new Discipline();
                        newDiscipline.Name = secondLayers.At(j).Name;
                        var thirdLayers = secondLayers.At(j).Sublayers;
                        for (ulong k = 0; k < thirdLayers.GetSize(); k++)
                        {
                            thirdLayers.At(k).IsVisible = true;
                            Category subCategory = new Category();
                            subCategory.Name = thirdLayers.At(k).Name;
                            newDiscipline.Categories.Add(subCategory);
                        }
                        disciplineCategoryData.Add(newDiscipline);
                    }
                }
            }
        }

        // Define the order
        Dictionary<string, int> DisciplineOrder = new Dictionary<string, int>
        {
            { "Architectural", 0 },
            { "Structural", 1 },
            { "Mechanical", 2 },
            { "Electrical", 3 },
            { "Piping", 4 }
        };

        // Order the disciplines by the predefined order
        disciplineCategoryData = disciplineCategoryData
            .OrderBy(d => DisciplineOrder.ContainsKey(d.Name) ? DisciplineOrder[d.Name] : int.MaxValue)
            .ToList();
    }

    void GetStatistics()
    {
        if (!buildingSceneLayer) { return; }
        var data = buildingSceneLayer.FetchStatisticsAsync();
        data.Wait();
        var stats = data.Get();
        stats.TryGetValue("BldgLevel", out var count);
        var bldgLevelMostFrequentValuesCollection = count.MostFrequentValues;
        List<int> bldgLevelValues = new List<int>();
        // Process BldgLevel statistics
        for (ulong i = 0; i < bldgLevelMostFrequentValuesCollection.GetSize(); i++)
        {
            var valueStr = bldgLevelMostFrequentValuesCollection.At(i);
            var valueInt = Convert.ToInt32(valueStr);
            bldgLevelValues.Add(valueInt);
        }

        // Determine the highest and lowest values for BldgLevel
        if (bldgLevelValues.Count > 0)
        {
            buildingStatistics.bldgLevelMin = bldgLevelValues.Min();
            buildingStatistics.bldgLevelMax = bldgLevelValues.Max();
        }

        // Process CreatedPhase statistics
        stats.TryGetValue("CreatedPhase", out var phaseCount);
        var createdPhaseMostFrequentValuesCollection = phaseCount.MostFrequentValues;
        List<int> createdPhaseValues = new List<int>();
        for (ulong i = 0; i < createdPhaseMostFrequentValuesCollection.GetSize(); i++)
        {
            var valueStr = createdPhaseMostFrequentValuesCollection.At(i);
            var valueInt = Convert.ToInt32(valueStr);
            createdPhaseValues.Add(valueInt);
        }

        // Determine the highest and lowest values for CreatedPhase
        if (createdPhaseValues.Count > 0)
        {
            buildingStatistics.createdPhaseMin = createdPhaseValues.Min();
            buildingStatistics.createdPhaseMax = createdPhaseValues.Max();
            String denomText = "/" + buildingStatistics.createdPhaseMax.ToString();
            Denom.text = denomText;
        }

        // Calculate the range for CreatedPhase
        int range = buildingStatistics.createdPhaseMax - buildingStatistics.createdPhaseMin;

        // Determine the step size based on the range
        if (range > 0)
        {
            if (buildingStatistics.createdPhaseMin == 0)
            {
                buildingStatistics.stepSize = 1.0f / range;
            }
            else if (buildingStatistics.createdPhaseMin == 1)
            {
                buildingStatistics.stepSize = 1.0f / (range + 1);
            }
        }
        else
        {
            buildingStatistics.stepSize = 1;
        }
    }
    public void PopulateSublayerMaps(string option, bool visible)
    {
        if (buildingSceneLayer != null)
        {
            var firstLayers = buildingSceneLayer.Sublayers;
            for (ulong i = 0; i < firstLayers.GetSize(); i++)
            {
                if (firstLayers.At(i).Name == "Full Model")
                {
                    var secondLayers = firstLayers.At(i).Sublayers;
                    for (ulong j = 0; j < secondLayers.GetSize(); j++)
                    {
                        if (option == secondLayers.At(j).Name)
                        {
                            secondLayers.At(j).IsVisible = visible;
                            break;
                        }
                        var thirdLayers = secondLayers.At(j).Sublayers;
                        for (ulong k = 0; k < thirdLayers.GetSize(); k++)
                        {
                            if (option == thirdLayers.At(k).Name)
                            {
                                thirdLayers.At(k).IsVisible = visible;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public void GenerateWhereClause(int level, int phase, bool bClearLevel, bool bNoLevel)
    {
        ArcGISBuildingAttributeFilter Filter = buildingSceneLayer.ActiveBuildingAttributeFilter;
        string BuildingLevels = "('" + level.ToString() + "')";
        string ConstructionPhases = "('";

        for (int i = buildingStatistics.createdPhaseMin; i <= phase; ++i)
        {
            string PhaseNum = i.ToString();
            ConstructionPhases += PhaseNum;
            if (i != phase)
            {
                ConstructionPhases += "', '";
            }
            else
            {
                ConstructionPhases += "')";
            }
        }

        // Create the where clauses
        string BuildingLevelClause = string.Format("BldgLevel in {0}", BuildingLevels);
        string ConstructionPhaseClause = string.Format("CreatedPhase in {0}", ConstructionPhases);
        string WhereClause = ConstructionPhaseClause;

        if (!bClearLevel)
        {
            WhereClause = string.Format("{0} and {1}", BuildingLevelClause, ConstructionPhaseClause);

        }
        if (bNoLevel)
        {
            WhereClause = ConstructionPhaseClause;
        }

        Filter.SolidFilterDefinition.WhereClause = WhereClause;

        buildingSceneLayer.ActiveBuildingAttributeFilter = Filter;

        Debug.Log(WhereClause);

    }
}
public class Discipline
{
    public string Name;
    public List<Category> Categories = new List<Category>();
}
public class Category
{
    public string Name;
}
public class BuildingStatistics
{
    public int bldgLevelMin;
    public int bldgLevelMax;
    public int createdPhaseMin;
    public int createdPhaseMax;
    public float stepSize;
}
