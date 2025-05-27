using BHR;
using System;
using System.Collections;
using UnityEngine;

public class CharacterBehavior : MonoBehaviour
{
    #region Dependencies

    private Rigidbody m_rigidbody;
    private CharacterGameplayData m_gameplayData;

    #endregion

    private Vector3 currentVelocity;
    private bool m_isInitialized = false;

    public Action OnThrowInput;

    private float m_moveLockTimer = 0f;

    public void InitializeDependencies(CharacterGameplayData a_gameplayData)
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_gameplayData = a_gameplayData;

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
        m_isPaused = true;

        m_oldVelocity = m_rigidbody.linearVelocity;
        m_oldAngularVelocity = m_rigidbody.angularVelocity;
        m_rigidbody.isKinematic = true;
    }

    private void OnResume()
    {
        m_rigidbody.isKinematic = false;
        m_rigidbody.linearVelocity = m_oldVelocity;
        m_rigidbody.angularVelocity = m_oldAngularVelocity;

        m_isPaused = false;
    }

    #endregion

    #region Life Cycle

    private Vector3 m_gravity = Physics.gravity;

    private void FixedUpdate()
    {
        if (!m_isInitialized || m_isPaused) return;

        Vector3 gravityForce = m_gravity * m_gameplayData.CharacterGravityScale;
        m_rigidbody.AddForce(gravityForce);

        // Lock the move when Singularity Jump is performed
        if (m_moveLockTimer > 0f)
        {
            m_moveLockTimer -= Time.fixedDeltaTime;
            if (m_moveLockTimer <= 0f)
            {
                m_moveLockTimer = 0f;
            }
        }
    }

    #endregion

    private void ResetVelocity()
    {
        if (!m_isInitialized) return;

        m_rigidbody.linearVelocity = Vector3.zero;
        m_rigidbody.angularVelocity = Vector3.zero;
    }

    public void ImobilizeCharacter(bool a_shouldBeImobilized)
    {
        m_rigidbody.isKinematic = a_shouldBeImobilized;
    }

    #region Movement
    public void Move(Vector2 a_moveValue)
    {
        if (m_rigidbody.isKinematic || m_moveLockTimer > 0f) return;

        float moveX = a_moveValue.x;
        float moveZ = a_moveValue.y;

        Transform cam = CameraManager.Instance.CurrentCam.transform;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * moveZ + camRight * moveX).normalized;
        Vector3 targetVelocity = moveDir * m_gameplayData.PlayerSpeed;

        if (!IsGrounded())
        {
            targetVelocity *= m_gameplayData.AirPlayerSpeedMultiplier;
        }

        m_rigidbody.linearVelocity = Vector3.SmoothDamp(m_rigidbody.linearVelocity, new Vector3(targetVelocity.x, m_rigidbody.linearVelocity.y, targetVelocity.z), ref currentVelocity, 0.1f);
    }

    #endregion

    #region Jump

    public void OnJump()
    {
        if (!IsGrounded()) return;

        m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, 0f, m_rigidbody.linearVelocity.z);
        m_rigidbody.AddForce(Vector3.up * m_gameplayData.JumpForce, ForceMode.Impulse);

        // Reset dash cooldown if jumping
        if (m_dashCooldownCoroutine != null)
        {
            StopCoroutine(m_dashCooldownCoroutine);
            m_canDash = true;
        }
    }

    public void OnSingularityJump(Vector3 a_linearVelocityToApply)
    {
        m_moveLockTimer = 0.5f;

        m_rigidbody.linearVelocity = a_linearVelocityToApply;

        m_rigidbody.AddForce(Vector3.up * m_gameplayData.SingularityJumpForce, ForceMode.Impulse);
    }

    #endregion

    #region Dash

    private bool m_canDash = true;
    private Coroutine m_dashCooldownCoroutine;
    public void OnDash()
    {
        if (!m_canDash) return;

        Transform cam = CameraManager.Instance.CurrentCam.transform;

        Vector3 dashDir = cam.forward;
        dashDir.y = 0;
        dashDir.Normalize();

        m_rigidbody.AddForce(dashDir * m_gameplayData.DashForce, ForceMode.VelocityChange);

        m_dashCooldownCoroutine = StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        m_canDash = false;

        yield return new WaitForSeconds(m_gameplayData.DashCooldown);

        m_canDash = true;
    }

    public void OnSingularityDash(Vector3 a_linearVelocityToApply, Vector3 a_direction)
    {
        m_moveLockTimer = 0.5f;
        m_rigidbody.linearVelocity = a_linearVelocityToApply;

        m_rigidbody.AddForce(a_direction * m_gameplayData.SingularityDashForce, ForceMode.VelocityChange);
    }

    #endregion

    #region Throw Singularity

    public void OnThrowSingularity()
    {
        ResetVelocity();
        OnThrowInput?.Invoke();
    }

    #endregion

    #region IsGrounded

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    public bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    #endregion
}
