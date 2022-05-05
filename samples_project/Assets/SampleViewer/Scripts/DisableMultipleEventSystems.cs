using System.Collections;
using System.Collections.Generic;
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
