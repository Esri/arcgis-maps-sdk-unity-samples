// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.EventSystems;

public class DisableMultipleEventSystems : MonoBehaviour
{
    void Start()
    {
        // If there are multiple EventSystems after we add the new scene disable them
        var EventSystems = FindObjectsOfType<EventSystem>();
        if (EventSystems.Length > 1)
        {
            foreach (var EventSystem in EventSystems)
            {
                if (EventSystem.name != "SampleViewerEventSystem")
                {
                    EventSystem.enabled = false;
                }
            }
        }
    }
}
