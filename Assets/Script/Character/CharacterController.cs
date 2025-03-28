using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed = 5;
    public float jumpForce = 5;
    public float dashForce = 5;

    private float dashCooldown = 1.5f;
    private float lastDashTime = 0;

    private Rigidbody rb;
    private CharacterBehavior characterBehavior;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        characterBehavior = GetComponent<CharacterBehavior>();
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

        Vector3 moveDirection = (camForward * moveZ + camRight * moveX).normalized * playerSpeed * Time.deltaTime;

        transform.position += moveDirection;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x + 0.1f);

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown)
        {
            rb.AddForce(activeCamera.forward * (dashForce + 5f), ForceMode.Impulse);
            print("Dash");
            lastDashTime = Time.time;
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Throw Singularity
        if (Input.GetKeyDown(KeyCode.E))
        {
            characterBehavior.ThrowSingularity();
        }
    }
}
