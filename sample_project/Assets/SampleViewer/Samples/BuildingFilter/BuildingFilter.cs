// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine;
using Esri.GameEngine.Layers;
using Esri.GameEngine.Layers.BuildingScene;
using Esri.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingFilter : MonoBehaviour
{
    [SerializeField] private Button addNewBSL;
    [SerializeField] private ArcGISLocationComponent cameraLocation;
    [SerializeField] private Toggle disciplineToggle;
    [SerializeField] private TextMeshProUGUI Denom;
    [SerializeField] private Button disableAll;
    [SerializeField] private Button enableAll;
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
    private BuildingStatistics buildingStatistics = new BuildingStatistics();
    public List<Discipline> DisciplineCategoryData = new List<Discipline>();
    private bool initialized = false;

    // Start is called before the first frame update
    private void Start()
    {
        disciplineToggle.onValueChanged.AddListener(delegate (bool active)
        {
            if (initialized == false)
            {
                InitializeBuildingSceneLayer();
                initialized = true;
            }
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
            NewBuildingSceneLayer(serviceURL.text.ToString());
        });
        enableAll.onClick.AddListener(delegate
        {
            foreach (var item in contentBoxes.contentList)
            {
                item.GetComponentInChildren<Toggle>().isOn = true;
            }
        });
        disableAll.onClick.AddListener(delegate
        {
            foreach (var item in contentBoxes.contentList)
            {
                item.GetComponentInChildren<Toggle>().isOn = false;
            }
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

    private void InitializeBuildingSceneLayer()
    {
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();
        if (arcGISMapComponent == null)
        {
            return;
        }
        var allLayers = arcGISMapComponent.Map.Layers;
        for (ulong i = 0; i < allLayers.GetSize(); i++)
        {
            if (allLayers.At(i).GetType().Name == "ArcGISBuildingSceneLayer")
            {
                buildingSceneLayer = allLayers.At(i) as ArcGISBuildingSceneLayer;
                break;
            }
        }
    }

    private void NewBuildingSceneLayer(string source)
    {
        var newLayer = new Esri.GameEngine.Layers.ArcGISBuildingSceneLayer(source, "UserBSL", 1.0f, true, "");
        var solidDef = new ArcGISSolidBuildingFilterDefinition("", "");
        var filter = new ArcGISBuildingAttributeFilter("Filter", "", solidDef);
        arcGISMapComponent.Map.Layers.Add(newLayer);
        StartCoroutine(Delay(5, false, newLayer, solidDef, filter));
    }

    private IEnumerator Delay(int time, bool isAdded, ArcGISBuildingSceneLayer newLayer, ArcGISSolidBuildingFilterDefinition solidDef, ArcGISBuildingAttributeFilter filter)
    {
        loadingText.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);

        if (!isAdded)
        {
            buildingSceneLayer = arcGISMapComponent.Map.Layers.Last() as ArcGISBuildingSceneLayer;
            if (buildingSceneLayer != null)
            {
                var firstLayers = buildingSceneLayer.Sublayers;
                for (ulong i = 0; i < firstLayers.GetSize(); i++)
                {
                    if (firstLayers.At(i).Name == "Full Model")
                    {
                        firstLayers.At(i).IsVisible = true;
                    }
                    else if (firstLayers.At(i).Name == "Overview")
                    {
                        firstLayers.At(i).IsVisible = false;
                    }
                }
            }
        }

        switch (buildingSceneLayer.LoadStatus)
        {
            case ArcGISLoadStatus.Loading:
                StartCoroutine(Delay(time, true, newLayer, solidDef, filter));
                break;

            case ArcGISLoadStatus.Loaded:
                loadingText.gameObject.SetActive(false);
                AddDisciplineCategoryData();
                contentBoxes.RemoveDisciplines();
                contentBoxes.AddDisciplines(DisciplineCategoryData);
                GetStatistics();
                filter.SolidFilterDefinition = solidDef;
                buildingSceneLayer.BuildingAttributeFilters.Add(filter);
                buildingSceneLayer.ActiveBuildingAttributeFilter = filter;
                var layerCenter = buildingSceneLayer.Extent.Center;
                arcGISMapComponent.OriginPosition = layerCenter;
                yield return new WaitForSeconds(1);
                cameraLocation.Position = layerCenter;
                phaseText.text = buildingStatistics.createdPhaseMax.ToString();
                trashButton.onClick.Invoke();
                GenerateWhereClause(buildingStatistics.bldgLevelMax, buildingStatistics.createdPhaseMax, true, true);
                break;

            default:
                var index = arcGISMapComponent.Map.Layers.GetSize() - 1;
                arcGISMapComponent.Map.Layers.Remove(index);
                buildingSceneLayer = arcGISMapComponent.Map.Layers.Last() as ArcGISBuildingSceneLayer;
                loadingText.gameObject.SetActive(false);
                failedToLoadText.gameObject.SetActive(true);
                yield return new WaitForSeconds(5);
                failedToLoadText.gameObject.SetActive(false);
                break;
        }
    }

    private void AddDisciplineCategoryData()
    {
        DisciplineCategoryData.Clear();
        if (buildingSceneLayer == null) return;

        var firstLayers = buildingSceneLayer.Sublayers;
        for (ulong i = 0; i < firstLayers.GetSize(); i++)
        {
            if (firstLayers.At(i).Name != "Full Model") continue;

            var secondLayers = firstLayers.At(i).Sublayers;
            for (ulong j = 0; j < secondLayers.GetSize(); j++)
            {
                secondLayers.At(j).IsVisible = true;
                Discipline newDiscipline = new Discipline
                {
                    Name = secondLayers.At(j).Name
                };

                var thirdLayers = secondLayers.At(j).Sublayers;
                for (ulong k = 0; k < thirdLayers.GetSize(); k++)
                {
                    thirdLayers.At(k).IsVisible = true;
                    Category subCategory = new Category
                    {
                        Name = thirdLayers.At(k).Name
                    };
                    newDiscipline.Categories.Add(subCategory);
                }
                DisciplineCategoryData.Add(newDiscipline);
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
        DisciplineCategoryData = DisciplineCategoryData
            .OrderBy(d => DisciplineOrder.ContainsKey(d.Name) ? DisciplineOrder[d.Name] : int.MaxValue)
            .ToList();
    }

    private void GetStatistics()
    {
        if (!buildingSceneLayer) { return; }
        var data = buildingSceneLayer.FetchStatisticsAsync();
        data.Wait();
        var stats = data.Get();
        stats.TryGetValue("BldgLevel", out var count);
        var bldgLevelMostFrequentValuesCollection = count.MostFrequentValues;
        var bldgLevelValues = new List<int>();

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
        var createdPhaseValues = new List<int>();
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
            var denomText = $"/{buildingStatistics.createdPhaseMax}";
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
        if (buildingSceneLayer == null) return;

        var firstLayers = buildingSceneLayer.Sublayers;
        for (ulong i = 0; i < firstLayers.GetSize(); i++)
        {
            if (firstLayers.At(i).Name != "Full Model") continue;

            var secondLayers = firstLayers.At(i).Sublayers;
            for (ulong j = 0; j < secondLayers.GetSize(); j++)
            {
                if (option == secondLayers.At(j).Name)
                {
                    secondLayers.At(j).IsVisible = visible;
                    return; // Exit the method once the option is found and visibility is set
                }

                var thirdLayers = secondLayers.At(j).Sublayers;
                for (ulong k = 0; k < thirdLayers.GetSize(); k++)
                {
                    if (option == thirdLayers.At(k).Name)
                    {
                        thirdLayers.At(k).IsVisible = visible;
                        return; // Exit the method once the option is found and visibility is set
                    }
                }
            }
        }
    }

    public void GenerateWhereClause(int level, int phase, bool clearLevel, bool noLevel)
    {
        ArcGISBuildingAttributeFilter Filter = buildingSceneLayer.ActiveBuildingAttributeFilter;
        string BuildingLevels = $"('{level}')";
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
        string BuildingLevelClause = $"BldgLevel in {BuildingLevels}";
        string ConstructionPhaseClause = $"CreatedPhase in {ConstructionPhases}";
        if (!clearLevel && !noLevel)
        {
            Filter.SolidFilterDefinition.WhereClause = $"{BuildingLevelClause} and {ConstructionPhaseClause}";
        }
        else
        {
            Filter.SolidFilterDefinition.WhereClause = ConstructionPhaseClause;
        }

        buildingSceneLayer.ActiveBuildingAttributeFilter = Filter;
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