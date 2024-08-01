// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SetTurnType : MonoBehaviour
{
    [Header("--------Turn Scripts--------")]
    [SerializeField] private ActionBasedContinuousTurnProvider continuousTurn;

    [SerializeField] private ActionBasedSnapTurnProvider snapTurn;

    public delegate void ChangedTurnType(bool type);

    public event ChangedTurnType OnTypeChanged;

    public void SetSmoothTurnSpeed(float speed)
    {
        continuousTurn.turnSpeed = speed;
    }

    public void SetSnapTurnSpeed(float snapSpeed)
    {
        snapTurn.turnAmount = snapSpeed;
    }

    public void ToggleSmoothTurn(bool isSmooth)
    {
        SetTurnTypeFromIndex(isSmooth);
        OnTypeChanged(isSmooth);
    }

    private void SetTurnTypeFromIndex(bool isSmooth)
    {
        snapTurn.enabled = !isSmooth;
        continuousTurn.enabled = isSmooth;
    }
}