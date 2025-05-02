using BHR;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 currentVelocity;
    private Vector2 moveValue;
    private bool isGrounded;
    private float lastDashTime = 0f;

    private GameplayData m_gameplayData;

    private TimeControl timeController;

    private int aimCallCount = 0;

    [SerializeField] private float fallMultiplier = 3f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        m_gameplayData = CharactersManager.Instance.GameplayData;
        rb.freezeRotation = true;

        timeController = GameManager.Instance.gameObject.GetComponent<TimeControl>();
    }

    private void OnEnable()
    {
        // Bind inputs 
        PlayersInputManager.Instance.OnHMove.AddListener(HandleMove);
        PlayersInputManager.Instance.OnHJump.AddListener(HandleJump);
        PlayersInputManager.Instance.OnHDash.AddListener(HandleDash);
        PlayersInputManager.Instance.OnHThrow.AddListener(HandleThrowSingularity);
        PlayersInputManager.Instance.OnHAim.AddListener(HandleAim);
        //PlayersInputManager.Instance.OnHSlide.AddListener();

    }

    private void OnDisable()
    {
        // Debind inputs
        PlayersInputManager.Instance.OnHMove.RemoveListener(HandleMove);
        PlayersInputManager.Instance.OnHJump.RemoveListener(HandleJump);
        PlayersInputManager.Instance.OnHDash.RemoveListener(HandleDash);
        PlayersInputManager.Instance.OnHThrow.RemoveListener(HandleThrowSingularity);
        PlayersInputManager.Instance.OnHAim.RemoveListener(HandleAim);
        //PlayersInputManager.Instance.OnHSlide.RemoveListener();
    }

    void FixedUpdate()
    {
        ApplyBetterGravity();

        if (CharactersManager.Instance.isSingularityThrown) return;

        Move();
    }

    void Update()
    {
        if (CharactersManager.Instance.isSingularityThrown) return;
    }

    private void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    public void HandleMove(Vector2 value)
    {
        moveValue = value;
    }

    public void Move()
    {
        float moveX = moveValue.x;
        float moveZ = moveValue.y;

        Transform cam = CharactersManager.Instance.isSingularityThrown
            ? CameraManager.Instance.SingularityCam.transform
            : CameraManager.Instance.PlayerCam.transform;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * moveZ + camRight * moveX).normalized;
        Vector3 targetVelocity = moveDir * m_gameplayData.PlayerSpeed;

        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z), ref currentVelocity, 0.1f);

        isGrounded = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - transform.localScale.y, transform.position.z), .3f, LayerMask.GetMask("ground"));
    }

    public void HandleJump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * m_gameplayData.JumpForce, ForceMode.Impulse);
        }
    }

    public void HandleDash()
    {
        if (Time.time >= lastDashTime + m_gameplayData.DashCooldown)
        {
            Transform cam = CharactersManager.Instance.isSingularityThrown
                ? CameraManager.Instance.SingularityCam.transform
                : CameraManager.Instance.PlayerCam.transform;

            Vector3 dashDir = cam.forward;
            dashDir.y = 0;
            dashDir.Normalize();

            rb.AddForce(dashDir * m_gameplayData.DashForce, ForceMode.VelocityChange);
            lastDashTime = Time.time;
        }
    }

    public void HandleThrowSingularity()
    {
        if (!timeController.isFinished)
        {
            timeController.isSlowed = false;
        }
         CharactersManager.Instance.TryThrowSingularity();
    }

    public void HandleAim()
    {
        if (aimCallCount == 0)
        {
            aimCallCount++;

            StartCoroutine(timeController.SlowmotionSequence());
            timeController.isStarted = true;

        }
        else if (aimCallCount == 3)
        {
            timeController.isStarted = false;
            timeController.isSlowed = false;

            aimCallCount = 0;
        }
        else
        {
            aimCallCount++;
        }

    }
}
