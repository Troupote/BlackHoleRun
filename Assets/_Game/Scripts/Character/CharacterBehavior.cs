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
    private bool m_wasInAir = false;

    private void FixedUpdate()
    {
        if (!m_isInitialized || m_isPaused) return;

        if ((!m_isGrounded || !m_isGroundedForJump) && !_isDashing)
        {
            Vector3 gravityForce = m_gravity * m_gameplayData.CharacterGravityScale * GameManager.Instance.GameTimeScale;
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
        if (_isDashing)
        {
            _dashDurationTimer -= Time.fixedDeltaTime * GameManager.Instance.GameTimeScale;
            if (_dashDurationTimer <= 0f)
            {
                _dashDurationTimer = 0f;
                _isDashing = false;
            }
        }

        if (_isDashing) // Probably not the best way to fix issue of high Y velocity when dashing on curved platforms, but that seems to work
        {
            if (m_rigidbody.linearVelocity.y >= 10f)
            {
                m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, 10f, m_rigidbody.linearVelocity.z);
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

        if (CanJump())
        {
            CharactersManager.Instance.LimitPlayersMovements.OnCharacterMovementTypeDone(LimitPlayersMovementsController.CharacterMovementType.Jump);
        }

        IsJumping = !CanJump() && m_rigidbody.linearVelocity.y > 0f;

        if (IsDashing)
        {
            RaycastHit hit;
            Vector3 origin = transform.position;

            if (Physics.SphereCast(origin, m_gameplayData.RaycastDashAsteroid, CameraManager.Instance.PlayerCam.transform.forward, out hit, 4f, m_gameplayData.GroundMask))
            {
                AsteroidBreak asteroid = hit.collider.GetComponent<AsteroidBreak>();
                if (asteroid != null)
                {
                    asteroid.DisableObject(true, true);
                }
            }
        }


        m_wasInAir = !m_isGroundedForJump;
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
        Vector3 targetVelocity = moveDir * m_gameplayData.PlayerSpeed * GameManager.Instance.GameTimeScale;

        if (!m_isGrounded)
        {
            targetVelocity *= m_gameplayData.AirPlayerSpeedMultiplier;
        }

        m_rigidbody.linearVelocity = Vector3.SmoothDamp(m_rigidbody.linearVelocity, new Vector3(targetVelocity.x, m_rigidbody.linearVelocity.y, targetVelocity.z), ref currentVelocity, 0.1f);
    }

    #endregion

    #region Jump

    internal bool IsJumping { get; private set; } = false;
    public void OnJump()
    {
        if (!CanJump()) return;

        m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, 0f, m_rigidbody.linearVelocity.z);
        m_rigidbody.AddForce(Vector3.up * m_gameplayData.JumpForce, ForceMode.Impulse);

        // Reset dash cooldown if jumping
        if (m_dashCooldownCoroutine != null)
        {
            StopCoroutine(m_dashCooldownCoroutine);
            m_canDash = true;
            CharactersManager.Instance.LimitPlayersMovements.OnCharacterMovementTypeDone(LimitPlayersMovementsController.CharacterMovementType.Dash);
        }

        m_coyotteTimeTimer = -1f;
        Invoke("ResetCoyotteTimer", 0.2f);
    }

    public bool CanJump() => (m_isGroundedForJump || m_coyotteTimeTimer > 0f) && !CharactersManager.Instance.isHumanoidAiming;

    private void CheckCoyotteTime()
    {
        if (m_coyotteTimeTimer < 0f) return;

        if (m_isGroundedForJump && m_coyotteTimeTimer != m_gameplayData.CoyotteTime)
            m_coyotteTimeTimer = m_gameplayData.CoyotteTime;
        else if (!m_isGroundedForJump && m_coyotteTimeTimer > 0f)
            m_coyotteTimeTimer -= Time.deltaTime * GameManager.Instance.GameTimeScale;
    }

    private void ResetCoyotteTimer() => m_coyotteTimeTimer = 0f;

    public void OnSingularityJump(Vector3 a_linearVelocityToApply)
    {
        m_moveLockTimer = 0.5f;

        m_rigidbody.linearVelocity = a_linearVelocityToApply;

        m_rigidbody.AddForce(Vector3.up * m_gameplayData.SingularityJumpForce * GameManager.Instance.GameTimeScale, ForceMode.Impulse);
    }

    #endregion

    #region Dash

    private bool m_canDash = true;
    private Coroutine m_dashCooldownCoroutine;
    internal bool IsDashing => _isDashing;
    public void OnDash()
    {
        if (!m_canDash || CharactersManager.Instance.isHumanoidAiming) return;

        CharactersManager.Instance.CancelAim();
        
        // Appel de PlayDashSound avec la position actuelle du personnage
        CharactersManager.Instance.PlayDashSound();

        GameManager.Instance.ChangeSpeedLines(SpeedLinesState.DASH);

        Transform cam = CameraManager.Instance.CurrentCam.transform;

        Vector3 dashDir = cam.forward;
        dashDir.y = 0f;
        dashDir.Normalize();

        Vector3 dashVelocity = dashDir * m_gameplayData.DashForce;
        m_rigidbody.linearVelocity = new Vector3(dashVelocity.x, 0f, dashVelocity.z);

        _isDashing = true;
        _dashDurationTimer = CharactersManager.Instance.GameplayData.DashDuration;

        if(m_isGrounded)
            m_dashCooldownCoroutine = StartCoroutine(GroundedDashCooldown());
        else
            m_dashCooldownCoroutine = StartCoroutine(AirDashCooldown());
    }

    private IEnumerator GroundedDashCooldown()
    {
        m_canDash = false;

        yield return new WaitForSeconds(m_gameplayData.DashCooldown / GameManager.Instance.GameTimeScale);

        m_canDash = true;
        CharactersManager.Instance.LimitPlayersMovements.OnCharacterMovementTypeDone(LimitPlayersMovementsController.CharacterMovementType.Dash);
    }

    private IEnumerator AirDashCooldown()
    {
        m_canDash = false;
        yield return new WaitUntil(() => m_isGroundedForJump);
        m_canDash = true;
    }

    public void OnSingularityDash(Vector3 a_linearVelocityToApply, Vector3 a_direction)
    {
        m_moveLockTimer = 0.5f;
        m_rigidbody.linearVelocity = new Vector3(a_linearVelocityToApply.x, a_linearVelocityToApply.y * 0f, a_linearVelocityToApply.z);


        _isDashing = true;
        _dashDurationTimer = CharactersManager.Instance.GameplayData.DashDuration;
        m_dashCooldownCoroutine = StartCoroutine(AirDashCooldown());

        m_rigidbody.AddForce(a_direction * m_gameplayData.SingularityDashForce, ForceMode.VelocityChange);
    }

    #endregion

    #region Throw Singularity

    public void OnThrowSingularity()
    {
        if (!CharactersManager.Instance.CanThrow) return;

        CharactersManager.Instance.CancelAim();

        RegisterActionsToLimitActions();

        ResetVelocity();
        ResetGroundedStates();
        m_canDash = true;

        OnThrowInput?.Invoke();
    }

    private void RegisterActionsToLimitActions()
    {
        if (!CharactersManager.Instance.GameplayData.ActivateMovementsLimit) return;

        if (!m_canDash)
            CharactersManager.Instance.LimitPlayersMovements.OnCharacterMovementTypePerformed(LimitPlayersMovementsController.CharacterMovementType.Dash);

        if (!CanJump())
            CharactersManager.Instance.LimitPlayersMovements.OnCharacterMovementTypePerformed(LimitPlayersMovementsController.CharacterMovementType.Jump);
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

    public void ResetGroundedStates()
    {
        m_isGrounded = false;
        m_isGroundedForJump = false;
        ResetCoyotteTimer();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_groundCheck.position, m_gameplayData.CapsuleGroundDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(m_groundCheck.position, Vector3.down * (m_gameplayData.CapsuleGroundDistance + m_gameplayData.RaycastGroundDistance));

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 4f);
        }
    }
#endif
    #endregion
}
