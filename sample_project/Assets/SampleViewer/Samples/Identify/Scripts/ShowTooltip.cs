// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ShowTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject toolTipPrefab;
    private GameObject toolTip;
    private float offset = -25;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (text.text.Length < 18)
        {
            return;
        }

        toolTip = Instantiate(toolTipPrefab);
        toolTip.transform.SetParent(text.gameObject.GetComponentInParent<Canvas>().transform);
        toolTip.GetComponentInChildren<TextMeshProUGUI>().text = text.text;
        toolTip.GetComponent<RectTransform>().position = new Vector2(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y + offset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(toolTip);
    }
}
