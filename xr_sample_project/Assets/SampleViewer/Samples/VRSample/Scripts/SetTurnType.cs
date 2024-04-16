using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SetTurnType : MonoBehaviour
{
    [Header("--------Turn Scripts--------")]
    [SerializeField] private ActionBasedContinuousTurnProvider continuousTurn;
    [SerializeField] private ActionBasedSnapTurnProvider snapTurn;

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
}
