// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentBoxes : MonoBehaviour
{
    [SerializeField] private GameObject disciplinePrefab;
    [SerializeField] private GameObject categoryPrefab;
    [SerializeField] private Transform contentTransform;
    [SerializeField] private RectTransform ScrollContent;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    private BuildingFilter filter;
    public List<GameObject> contentList = new List<GameObject>();

    // Start is called before the first frame update
    private void Start()
    {
        filter = FindObjectOfType<BuildingFilter>();
        AddDisciplines(filter.DisciplineCategoryData);
    }

    public void AddDisciplines(List<Discipline> data)
    {
        // Clear existing content
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // Add new content
        foreach (var discipline in data)
        {
            // Instantiate a discipline item
            GameObject disciplineItem = Instantiate(disciplinePrefab);
            disciplineItem.GetComponentInChildren<TextMeshProUGUI>().text = discipline.Name;
            disciplineItem.transform.SetParent(verticalLayoutGroup.transform, false);
            var disciplineContent = disciplineItem.GetComponent<RectTransform>();
            contentList.Add(disciplineItem);
            var disciplineLayout = disciplineItem.GetComponentInChildren<VerticalLayoutGroup>();

            disciplineContent.sizeDelta = new Vector2(disciplineContent.sizeDelta.x, disciplineContent.sizeDelta.y + 30);
            ScrollContent.sizeDelta = new Vector2(ScrollContent.sizeDelta.x, ScrollContent.sizeDelta.y + 30);

            foreach (var category in discipline.Categories)
            {
                GameObject categoryItem = Instantiate(categoryPrefab);
                categoryItem.transform.SetParent(disciplineLayout.transform, false);
                disciplineContent.sizeDelta = new Vector2(disciplineContent.sizeDelta.x, disciplineContent.sizeDelta.y + 16);
                ScrollContent.sizeDelta = new Vector2(ScrollContent.sizeDelta.x, ScrollContent.sizeDelta.y + 30);
                categoryItem.GetComponentInChildren<TextMeshProUGUI>().text = category.Name;
                contentList.Add(categoryItem);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollContent);
            ScrollContent.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 1f;
        }
    }

    public void RemoveDisciplines()
    {
        foreach (var item in contentList)
        {
            Destroy(item);
        }
    }
}