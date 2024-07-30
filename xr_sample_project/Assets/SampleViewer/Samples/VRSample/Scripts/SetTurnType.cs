using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SetTurnType : MonoBehaviour
{
    [Header("--------Turn Scripts--------")]
    [SerializeField] private ActionBasedContinuousTurnProvider continuousTurn;
    [SerializeField] private ActionBasedSnapTurnProvider snapTurn;

    public delegate void ChangedTurnType(bool type);
    public event ChangedTurnType OnTypeChanged;

    public void SetTurnTypeFromIndex(int index)
    {
        if (index == 0)
        {
            snapTurn.enabled = false;
            continuousTurn.enabled = true;
        }
        else if (index == 1)
        {
            snapTurn.enabled = true;
            continuousTurn.enabled = false;
        }
    }

    public void ToggleSmoothTurn(bool isSmooth)
    {
        SetTurnTypeFromIndex(isSmooth ? 0 : 1);
        OnTypeChanged(isSmooth);
    }

    public void SetSmoothTurnSpeed(float speed)
    {
        continuousTurn.turnSpeed = speed;
    }

    public void SetSnapTurnSpeed(float snapSpeed)
    {
        snapTurn.turnAmount = snapSpeed;
    }
}
