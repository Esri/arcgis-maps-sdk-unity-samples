using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class ContentBoxes : MonoBehaviour
{
    [SerializeField] private GameObject disciplinePrefab;
    [SerializeField] private GameObject categoryPrefab;
    [SerializeField] private GameObject panelPrefab;
    [SerializeField] private Transform contentTransform;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    private List<GameObject> contentList = new List<GameObject>();
    private BuildingFilter filter;
    [SerializeField] private RectTransform ScrollContent;

    // Start is called before the first frame update
    void Start()
    {
        filter = FindObjectOfType<BuildingFilter>();
        AddDisciplines(filter.disciplineCategoryData);
    }

    // Update is called once per frame
    void Update()
    {
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
            Debug.Log(discipline.Name);
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
                disciplineContent.sizeDelta = new Vector2(disciplineContent.sizeDelta.x, disciplineContent.sizeDelta.y + 15);
                ScrollContent.sizeDelta = new Vector2(ScrollContent.sizeDelta.x, ScrollContent.sizeDelta.y + 30);
                Debug.Log(ScrollContent.sizeDelta);
                categoryItem.GetComponentInChildren<TextMeshProUGUI>().text = category.Name;
                contentList.Add(categoryItem);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollContent);
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