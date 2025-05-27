using System;
using System.Runtime.CompilerServices;
using BHR;
using Cinemachine;
using UnityEngine;

public class SingularityBehavior : MonoBehaviour
{
    #region Dependencies
    private Rigidbody m_rigidbody;
    private CharacterGameplayData m_gameplayData;
    #endregion

    [field: SerializeField]
    internal SingularityCharacterFollowComponent SingularityCharacterFollowComponent { get; private set; } = null;

    public Action OnThrowPerformed;
    public Action OnUnmorph;
    public Action<Vector3> OnJump;
    public Action<Vector3, Vector3> OnDash;

    private bool m_isInitialized = false;

    public void InitializeDependencies(CharacterGameplayData a_gameplayData)
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_gameplayData = a_gameplayData;

        SingularityCharacterFollowComponent.InititializeDependencies(this.transform, m_rigidbody);

        ListenToEvents();

        m_isInitialized = true;
    }

    public void ListenToEvents()
    {
        GameManager.Instance.OnPaused.AddListener(OnPaused);
        GameManager.Instance.OnResumed.AddListener(OnResume);
    }

    public void UnlistenToEvents()
    {
        GameManager.Instance.OnPaused.RemoveListener(OnPaused);
        GameManager.Instance.OnResumed.RemoveListener(OnResume);
    }

    #region On Pause

    private bool m_isPaused = false;
    private Vector3 m_oldVelocity;
    private Vector3 m_oldAngularVelocity;

    private void OnPaused()
    {
        if (SingularityCharacterFollowComponent.IsPickedUp) return;

        m_isPaused = true;

        m_oldVelocity = m_rigidbody.linearVelocity;
        m_oldAngularVelocity = m_rigidbody.angularVelocity;
        m_rigidbody.isKinematic = true;
    }

    private void OnResume()
    {
        if (SingularityCharacterFollowComponent.IsPickedUp) return;

        m_rigidbody.isKinematic = false;
        m_rigidbody.linearVelocity = m_oldVelocity;
        m_rigidbody.angularVelocity = m_oldAngularVelocity;

        m_isPaused = false;
    }

    #endregion

    #region Life Cycle

    private void FixedUpdate()
    {
        if (!m_isInitialized || m_isPaused) return;

        HandleThrowCurve();

    }

    #endregion

    internal bool IsAllowedToBeThrown => SingularityCharacterFollowComponent.IsPickedUp;

    #region Move
    public void Move(Vector2 a_movementValue)
    {
        float moveX = a_movementValue.x;

        if (a_movementValue.x == 0) return;

        Vector3 curveDirection = transform.right * moveX;

        m_rigidbody.AddForce(curveDirection.normalized * CharactersManager.Instance.GameplayData.MovingCurveForce, ForceMode.Force);
    }

    #endregion

    #region Throw

    [SerializeField]
    private AnimationCurve m_verticalVelocityCurve;
    [SerializeField]
    private float m_curveDuration = 2f;
    private bool m_isThrown = false;
    private float m_throwTime = 0f;

    public void OnThrow()
    {
        SingularityCharacterFollowComponent.PickupSingularity(false);

        Vector3 throwDirection = CameraManager.Instance.MainCam.transform.forward;

        m_rigidbody.AddForce(throwDirection * m_gameplayData.ThrowForce, ForceMode.Impulse);

        m_throwTime = 0f;

        OnThrowPerformed?.Invoke();
    }

    private void HandleThrowCurve()
    {
        if (SingularityCharacterFollowComponent.IsPickedUp) return;

        m_throwTime += Time.fixedDeltaTime;
        float normalizedTime = m_throwTime / m_curveDuration;

        Vector3 velocity = m_rigidbody.linearVelocity;

        velocity.y += Physics.gravity.y * Time.fixedDeltaTime;

        /* Useless for the moment, idk yet
        float verticalMultiplier = m_verticalVelocityCurve.Evaluate(normalizedTime);
        velocity.y *= verticalMultiplier;
        */

        m_rigidbody.linearVelocity = velocity;

    }
    #endregion

    #region Jump

    public void Jump()
    {
        if (m_gameplayData.ActivateMovementsLimit && 
            CharactersManager.Instance.LimitPlayersMovements.HasPerformed(LimitPlayersMovementsController.CharacterMovementType.Jump) ||
            CharactersManager.Instance.LimitPlayersMovements.HasPerformedBoth()) return;

        OnJump?.Invoke(m_rigidbody.linearVelocity);
    }

    #endregion

    #region Dash

    public void Dash()
    {
        if (m_gameplayData.ActivateMovementsLimit &&
            CharactersManager.Instance.LimitPlayersMovements.HasPerformed(LimitPlayersMovementsController.CharacterMovementType.Dash) ||
            CharactersManager.Instance.LimitPlayersMovements.HasPerformedBoth()) return;

        Transform cam = CameraManager.Instance.CurrentCam.transform;

        Vector3 dashDir = cam.forward;
        dashDir.y = 0;
        dashDir.Normalize();

        OnDash?.Invoke(m_rigidbody.linearVelocity, dashDir);
    }

    #endregion

    #region Collision Detection

    private void OnCollisionEnter(Collision collision)
    {
        if (SingularityCharacterFollowComponent.IsPickedUp || CharactersManager.Instance.SingularityMovingToCharacter) return;

        m_isThrown = false;
        OnUnmorph?.Invoke();
    }

    #endregion
}
