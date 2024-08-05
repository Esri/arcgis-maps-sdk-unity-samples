using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisciplineButton : MonoBehaviour
{
    [SerializeField] private Toggle categoryToggle;
    [SerializeField] private RectTransform disciplineArea;
    [SerializeField] private Sprite expandSprite;
    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private Sprite minimizeSprite;
    [SerializeField] private Toggle selectionToggle;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private Sprite visibleSprite;
    private ContentBoxes contentBoxes;
    private BuildingFilter filter;
    private float originalHeight;

    // Start is called before the first frame update
    private void Start()
    {
        filter = FindObjectOfType<BuildingFilter>();
        contentBoxes = FindObjectOfType<ContentBoxes>();
        string text = GetComponentInChildren<TextMeshProUGUI>().text;

        if (categoryToggle != null)
        {
            originalHeight = disciplineArea.sizeDelta.y;
            categoryToggle.onValueChanged.AddListener(delegate (bool active)
            {
                categoryToggle.isOn = active;

                if (categoryToggle.isOn)
                {
                    disciplineArea.sizeDelta = new Vector2(disciplineArea.sizeDelta.x, originalHeight);
                    SetVerticalLayoutGroupHeight(true);
                    categoryToggle.GetComponentInChildren<Image>().sprite = expandSprite;
                }
                else
                {
                    originalHeight = disciplineArea.sizeDelta.y;
                    disciplineArea.sizeDelta = new Vector2(disciplineArea.sizeDelta.x, 10);
                    SetVerticalLayoutGroupHeight(false);
                    categoryToggle.GetComponentInChildren<Image>().sprite = minimizeSprite;
                }

                // Force layout refresh
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentBoxes.GetComponent<RectTransform>());
            });
        }

        selectionToggle.onValueChanged.AddListener(delegate (bool active)
        {
            selectionToggle.isOn = active;

            filter.PopulateSublayerMaps(text, selectionToggle.isOn);
            if (selectionToggle.isOn)
            {
                selectionToggle.GetComponentInChildren<Image>().sprite = visibleSprite;
                if (categoryToggle != null)
                {
                    categoryToggle.isOn = true;
                    foreach (var childToggle in verticalLayoutGroup.GetComponentsInChildren<Toggle>())
                    {
                        childToggle.isOn = true;
                    }
                }
            }
            else
            {
                selectionToggle.GetComponentInChildren<Image>().sprite = hiddenSprite;
                if (categoryToggle != null)
                {
                    categoryToggle.isOn = false;
                    foreach (var childToggle in verticalLayoutGroup.GetComponentsInChildren<Toggle>())
                    {
                        childToggle.isOn = false;
                    }
                }
            }
        });
    }

    // Method to set the height of the VerticalLayoutGroup
    private void SetVerticalLayoutGroupHeight(bool active)
    {
        RectTransform rectTransform = verticalLayoutGroup.GetComponent<RectTransform>();
        if (active)
        {
            rectTransform.localScale = Vector3.one;
        }
        else
        {
            rectTransform.localScale = Vector3.zero;
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}