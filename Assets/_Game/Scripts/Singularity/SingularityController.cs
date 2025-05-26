using BHR;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class SingularityController : MonoBehaviour
{
    private SingularityBehavior m_singularityBehavior;
    private Vector2 m_moveValue = Vector2.zero;
    private bool m_isInitialized = false;
    void Start()
    {
        InitializeDependencies();
    }

    private void InitializeDependencies()
    {
        m_singularityBehavior = GetComponent<SingularityBehavior>();

        m_isInitialized = true;
    }

    private void OnEnable()
    {
        // Bind inputs
        PlayersInputManager.Instance.OnSMove.AddListener(HandleSingularityMove);
        PlayersInputManager.Instance.OnSUnmorph.AddListener(HandleUnMorphEarly);
        PlayersInputManager.Instance.OnSJump.AddListener(HandleJump);
        PlayersInputManager.Instance.OnSDash.AddListener(HandleDash);
        CharactersManager.Instance.ResetInputs += ResetInputs;
        GameManager.Instance.OnPaused.AddListener(ResetInputs);
    }

    private void OnDisable()
    {
        // Debind inputs
        PlayersInputManager.Instance.OnSMove.RemoveListener(HandleSingularityMove);
        PlayersInputManager.Instance.OnSUnmorph.RemoveListener(HandleUnMorphEarly);
        PlayersInputManager.Instance.OnSJump.RemoveListener(HandleJump);
        PlayersInputManager.Instance.OnSDash.RemoveListener(HandleDash);
        CharactersManager.Instance.ResetInputs -= ResetInputs;
        GameManager.Instance.OnPaused.RemoveListener(ResetInputs);
    }

    #region Life Cycle

    private void FixedUpdate()
    {
        if (!m_isInitialized) return;

        m_singularityBehavior.Move(m_moveValue);
    }

    #endregion

    private void ResetInputs()
    {
        m_moveValue = Vector2.zero;
    }

    private void HandleSingularityMove(Vector2 a_movementValue)
    {
        m_moveValue = a_movementValue;
    }

    private void HandleUnMorphEarly()
    {
        m_singularityBehavior.OnUnmorph?.Invoke();
    }

    private void HandleJump()
    {
        m_singularityBehavior.Jump();
    }

    private void HandleDash()
    {
        m_singularityBehavior.Dash();
    }

}