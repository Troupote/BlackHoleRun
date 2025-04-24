using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 currentVelocity;
    private bool isGrounded;
    private float lastDashTime = 0f;

    private GameplayData m_gameplayData;

    [SerializeField] private float fallMultiplier = 12f;
    [SerializeField] private float lowJumpMultiplier = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        m_gameplayData = CharactersManager.Instance.GameplayData;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        Move();
        ApplyBetterGravity();
    }

    void Update()
    {
        HandleJump();
        HandleDash();
        HandleSingularity();
    }

    private void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Transform cam = CharactersManager.Instance.SingularityThrown
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

        isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.y + 0.2f);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * m_gameplayData.JumpForce, ForceMode.Impulse);
        }
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + m_gameplayData.DashCooldown)
        {
            Transform cam = CharactersManager.Instance.SingularityThrown
                ? CameraManager.Instance.SingularityCam.transform
                : CameraManager.Instance.PlayerCam.transform;

            Vector3 dashDir = cam.forward;
            dashDir.y = 0;
            dashDir.Normalize();

            rb.AddForce(dashDir * m_gameplayData.DashForce, ForceMode.VelocityChange);
            lastDashTime = Time.time;
        }
    }

    private void HandleSingularity()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CharactersManager.Instance.TryThrowSingularity();
        }
    }
}
