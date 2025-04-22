using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    private float lastDashTime = 0;

    private Rigidbody rb;
    private bool isGrounded;

    private GameplayData m_gameplayData;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        m_gameplayData = CharactersManager.Instance.GameplayData;
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Transform activeCamera = CharactersManager.Instance.SingularityThrown
            ? CameraManager.Instance.SingularityCam.transform
            : CameraManager.Instance.PlayerCam.transform;

        Vector3 camForward = activeCamera.forward;
        Vector3 camRight = activeCamera.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveZ + camRight * moveX).normalized * m_gameplayData.PlayerSpeed * Time.deltaTime;

        transform.position += moveDirection;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x + 0.1f);

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + m_gameplayData.DashCooldown)
        {
            rb.AddForce(activeCamera.forward * (m_gameplayData.DashForce + 5f), ForceMode.Impulse);
            print("Dash");
            lastDashTime = Time.time;
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * m_gameplayData.JumpForce, ForceMode.Impulse);
        }

        // Throw Singularity
        if (Input.GetKeyDown(KeyCode.E))
        {
            CharactersManager.Instance.TryThrowSingularity();
        }
    }
}
