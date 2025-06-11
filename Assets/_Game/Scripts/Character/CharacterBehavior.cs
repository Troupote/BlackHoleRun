using BHR;
using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
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

    private bool _isDashing;
    private float _dashDurationTimer = 0f;

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

        if (!m_isGrounded)
        {
            Vector3 gravityForce = m_gravity * m_gameplayData.CharacterGravityScale * (_isDashing ? 0f : 1f);
            m_rigidbody.AddForce(gravityForce);
        }

        // Lock the move when Singularity Jump is performed
        if (m_moveLockTimer > 0f)
        {
            m_moveLockTimer -= Time.fixedDeltaTime * GameManager.Instance.GameTimeScale;
            if (m_moveLockTimer <= 0f)
            {
                m_moveLockTimer = 0f;
            }
        }

        // Dash duration timer
        if(_isDashing)
        {
            _dashDurationTimer -= Time.fixedDeltaTime * GameManager.Instance.GameTimeScale; 
            if( _dashDurationTimer <= 0f)
            {
                _dashDurationTimer = 0f;
                _isDashing = false;
            }
        }
    }

    private bool m_isGrounded;
    private bool m_isGroundedForJump;
    [SerializeField]
    private float m_coyotteTimeTimer = 0f;


    private void Update()
    {
        if (!m_isInitialized || m_isPaused) return;

        m_isGrounded = IsGrounded();
        m_isGroundedForJump = IsGroundedForJump();
        CheckCoyotteTime();
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

        if (!m_isGrounded)
        {
            targetVelocity *= m_gameplayData.AirPlayerSpeedMultiplier;
        }

        m_rigidbody.linearVelocity = Vector3.SmoothDamp(m_rigidbody.linearVelocity, new Vector3(targetVelocity.x, m_rigidbody.linearVelocity.y, targetVelocity.z), ref currentVelocity, 0.1f);
    }

    #endregion

    #region Jump

    public void OnJump()
    {
        if (!m_isGroundedForJump && m_coyotteTimeTimer <= 0f) return;


        m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, 0f, m_rigidbody.linearVelocity.z);
        m_rigidbody.AddForce(Vector3.up * m_gameplayData.JumpForce, ForceMode.Impulse);

        // Reset dash cooldown if jumping
        if (m_dashCooldownCoroutine != null)
        {
            StopCoroutine(m_dashCooldownCoroutine);
            m_canDash = true;
        }

        CharactersManager.Instance.LimitPlayersMovements.OnCharacterMovementTypePerformed(LimitPlayersMovementsController.CharacterMovementType.Jump);
        m_coyotteTimeTimer = -1f;
        Invoke("ResetCoyotteTimer", 0.2f);
    }

    private void CheckCoyotteTime()
    {
        if(m_coyotteTimeTimer < 0f) return;

        if (m_isGroundedForJump && m_coyotteTimeTimer != CharactersManager.Instance.GameplayData.CoyotteTime)
            m_coyotteTimeTimer = CharactersManager.Instance.GameplayData.CoyotteTime;
        else if (!m_isGroundedForJump && m_coyotteTimeTimer > 0f)
            m_coyotteTimeTimer -= Time.deltaTime * GameManager.Instance.GameTimeScale;
    }

    private void ResetCoyotteTimer() => m_coyotteTimeTimer = 0f;

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
        dashDir.y = 0f;
        dashDir.Normalize();

        Vector3 dashVelocity = dashDir * m_gameplayData.DashForce;
        m_rigidbody.linearVelocity = new Vector3(dashVelocity.x, m_rigidbody.linearVelocity.y, dashVelocity.z);

        _isDashing = true;
        _dashDurationTimer = CharactersManager.Instance.GameplayData.DashDuration;

        m_dashCooldownCoroutine = StartCoroutine(DashCooldown());

        CharactersManager.Instance.LimitPlayersMovements.OnCharacterMovementTypePerformed(LimitPlayersMovementsController.CharacterMovementType.Dash);
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
        if (!CharactersManager.Instance.CanThrow) return;

        ResetVelocity();
        OnThrowInput?.Invoke();
    }

    #endregion

    #region IsGrounded

    [SerializeField] private Transform m_groundCheck;

    public bool IsGrounded()
    {
        bool defaultCheck = Physics.CheckSphere(m_groundCheck.position, m_gameplayData.CapsuleGroundDistance, m_gameplayData.GroundMask);

        return defaultCheck;
    }

    public bool IsGroundedForJump()
    {
        return Physics.Raycast(m_groundCheck.position, Vector3.down, m_gameplayData.CapsuleGroundDistance + m_gameplayData.RaycastGroundDistance, m_gameplayData.GroundMask);
    }

    private void OnDrawGizmos()
    {
        if (m_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_groundCheck.position, m_gameplayData.CapsuleGroundDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(m_groundCheck.position, Vector3.down * (m_gameplayData.CapsuleGroundDistance + m_gameplayData.RaycastGroundDistance));
        }
    }

    #endregion
}
