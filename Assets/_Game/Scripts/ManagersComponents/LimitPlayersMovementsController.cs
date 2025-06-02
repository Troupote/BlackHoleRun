using BHR;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LimitPlayersMovementsController : MonoBehaviour
{
    private HashSet<CharacterMovementType> m_performedMovementTypes = new HashSet<CharacterMovementType>();

    public enum CharacterMovementType
    {
        Dash,
        Jump,
    }

    public void OnCharacterMovementTypePerformed(CharacterMovementType a_newMovementType)
    {
        if (!CharactersManager.Instance.GameplayData.ActivateMovementsLimit) return;

        m_performedMovementTypes.Add(a_newMovementType);
    }

    public bool HasPerformed(CharacterMovementType a_movementType)
    {
        return m_performedMovementTypes.Contains(a_movementType);
    }

    public bool HasPerformedBoth()
    {
        return m_performedMovementTypes.Contains(CharacterMovementType.Dash) &&
        m_performedMovementTypes.Contains(CharacterMovementType.Jump);
    }

    public void ClearPerformedMovements()
    {
        m_performedMovementTypes.Clear();
    }
}
