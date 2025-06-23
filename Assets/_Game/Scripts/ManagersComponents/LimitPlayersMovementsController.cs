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

    public void OnCharacterMovementTypeDone(CharacterMovementType a_movementType)
    {
        if (!CharactersManager.Instance.GameplayData.ActivateMovementsLimit) return;

        if (m_performedMovementTypes.Contains(a_movementType))
        {
            m_performedMovementTypes.Remove(a_movementType);
        }
    }

    public bool HasPerformed(CharacterMovementType a_movementType)
    {
        if (!CharactersManager.Instance.GameplayData.ActivateMovementsLimit) return false;

        return m_performedMovementTypes.Contains(a_movementType);
    }

    public bool HasPerformedBoth()
    {
        if (!CharactersManager.Instance.GameplayData.ActivateMovementsLimit) return false;

        return m_performedMovementTypes.Contains(CharacterMovementType.Dash) &&
        m_performedMovementTypes.Contains(CharacterMovementType.Jump);
    }

    public void ClearPerformedMovements()
    {
        if (!CharactersManager.Instance.GameplayData.ActivateMovementsLimit) return;

        m_performedMovementTypes.Clear();

    }
}
