using System;
using System.Collections;
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

    [field: SerializeField]
    internal SingularityShaderColorController ShaderColorController;

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

        if (CharactersManager.Instance.IsCurrentlySwitching)
            return;

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
        if (a_movementValue.x == 0) return;

        Vector3 velocity = m_rigidbody.linearVelocity;
        if (velocity.sqrMagnitude < 0.01f) return;

        Vector3 forwardDir = velocity.normalized;
        Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir).normalized;

        // Apply curve force
        Vector3 curveForce = rightDir * a_movementValue.x * CharactersManager.Instance.GameplayData.MovingCurveForce;

        m_rigidbody.AddForce(curveForce, ForceMode.Force);
    }


    #endregion

    #region Throw

    private bool m_isThrown = false;
    private float m_throwTime = 0f;

    public void OnThrow()
    {
        Debug.Log("SThrow performed");
        SingularityCharacterFollowComponent.PickupSingularity(false);

        Vector3 throwDirection = CameraManager.Instance.MainCam.transform.forward;

        StartCoroutine(AlignAndThrow(throwDirection));

        m_throwTime = 0f;
        OnThrowPerformed?.Invoke();
    }

    private IEnumerator AlignAndThrow(Vector3 direction)
    {
        m_rigidbody.isKinematic = true;

        float elapsedTime = 0f;

        Vector3 startPos = transform.position;
        Vector3 targetPos = CameraManager.Instance.MainCam.transform.position + direction * m_gameplayData.ThrowingCenterDistanceMultiplier;

        while (elapsedTime < m_gameplayData.TimeTakenToGoToCenter)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / m_gameplayData.TimeTakenToGoToCenter);
            elapsedTime += Time.deltaTime * GameManager.Instance.GameTimeScale;
            yield return null;
        }

        transform.position = targetPos;

        m_rigidbody.isKinematic = false;
        m_rigidbody.AddForce(direction * m_gameplayData.ThrowForce, ForceMode.Impulse);
    }


    private void HandleThrowCurve()
    {
        if (SingularityCharacterFollowComponent.IsPickedUp) return;

        //Debug.Log("Handling Throw Curve for Singularity");

        Vector3 velocity = m_rigidbody.linearVelocity;

        if (m_throwTime >= m_gameplayData.GravityDelayDuration)
        {
            velocity.y += Physics.gravity.y * Time.fixedDeltaTime * m_gameplayData.GravityMultiplier;
            m_rigidbody.linearVelocity = velocity;
        }

        m_throwTime += Time.fixedDeltaTime;

        /*
        var curveParam = m_gameplayData.GravityCurveMultiplier.Evaluate(m_throwTime / m_gameplayData.GravityDelayDuration);

        velocity.y += Physics.gravity.y * Time.fixedDeltaTime * curveParam;

        m_rigidbody.linearVelocity = velocity;

        Debug.DrawRay(transform.position, Vector3.down * curveParam, Color.blue, 0.1f);
        */
    }
    #endregion

    #region Jump

    public void Jump()
    {
        if (CharactersManager.Instance.LimitPlayersMovements.HasPerformed(LimitPlayersMovementsController.CharacterMovementType.Jump) ||
            CharactersManager.Instance.LimitPlayersMovements.HasPerformedBoth()) return;

        OnJump?.Invoke(m_rigidbody.linearVelocity);
    }

    #endregion

    #region Dash

    public void Dash()
    {
        if (CharactersManager.Instance.LimitPlayersMovements.HasPerformed(LimitPlayersMovementsController.CharacterMovementType.Dash) ||
            CharactersManager.Instance.LimitPlayersMovements.HasPerformedBoth()) return;

        Transform cam = CameraManager.Instance.CurrentCam.transform;

        Vector3 dashDir = cam.forward;
        dashDir.y = 0;
        dashDir.Normalize();

        OnDash?.Invoke(m_rigidbody.linearVelocity, dashDir);
    }

    #endregion

    private bool m_ignoreUnmorph = false;

    public void SetIgnoreCollision(bool shouldIgnore)
    {
        m_ignoreUnmorph = shouldIgnore;
        m_rigidbody.detectCollisions = !shouldIgnore;
    }


    #region Collision Detection

    private void OnCollisionEnter(Collision collision)
    {
        if (SingularityCharacterFollowComponent.IsPickedUp || m_ignoreUnmorph) return;

        m_isThrown = false;
        OnUnmorph?.Invoke();
    }

    public bool IsOverlapping()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1f, ~0, QueryTriggerInteraction.Ignore);
        foreach (var hit in hits)
        {
            if (hit.attachedRigidbody != m_rigidbody) // ignore self
                return true;
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
#endif
#endregion
}
