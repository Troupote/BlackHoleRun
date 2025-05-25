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

    private bool m_isInitialized = false;

    public void InitializeDependencies(CharacterGameplayData a_gameplayData)
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_gameplayData = a_gameplayData;

        SingularityCharacterFollowComponent.InititializeDependencies(this.transform, m_rigidbody);

        m_isInitialized = true;
    }
    #region Life Cycle

    private void FixedUpdate()
    {
        if (!m_isInitialized) return;

        HandleThrowCurve();
    }

    #endregion

    internal bool IsAllowedToBeThrown => SingularityCharacterFollowComponent.IsKinematicEnabled();

    public void Move(Vector2 a_movementValue)
    {
        float moveX = a_movementValue.x;

        if (a_movementValue.x == 0) return;

        Vector3 curveDirection = transform.right * moveX;

        m_rigidbody.AddForce(curveDirection.normalized * CharactersManager.Instance.GameplayData.MovingCurveForce, ForceMode.Force);
    }

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
        if (SingularityCharacterFollowComponent.IsKinematicEnabled()) return;

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
        OnJump?.Invoke(m_rigidbody.linearVelocity);
    }

    #endregion

    #region Collision Detection

    private void OnCollisionEnter(Collision collision)
    {
        if (SingularityCharacterFollowComponent.IsKinematicEnabled() || CharactersManager.Instance.SingularityMovingToCharacter) return;

        m_isThrown = false;
        OnUnmorph?.Invoke();
    }

    #endregion
}
