// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.EventSystems;

public class OnButtonDown : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    bool pressed;

    public enum type
    {
        ZoomIn,
        ZoomOut
    }

    public XRTableTopInteractor tableTopInteractor;
    public type Type;

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) 
    {
        pressed = false;
    }

    private void Update()
    {
        if (pressed)
        {
            if (Type == type.ZoomIn)
            {
                tableTopInteractor.ZoomMap(1.0f);
            }
            else if (Type == type.ZoomOut)
            {
                tableTopInteractor.ZoomMap(-1.0f);
            }
        }
    }
}
