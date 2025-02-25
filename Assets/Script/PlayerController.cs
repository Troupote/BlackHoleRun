using Cinemachine;
using Unity.PlasticSCM.Editor.UI;
using Unity.VisualScripting;
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
    private bool isGrounded;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(moveX, 0, moveZ) * playerSpeed * Time.deltaTime;

        move = transform.TransformDirection(move);
        transform.localPosition += move;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x + .1f);

        //Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown)
        {
            rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);
            print("Dash");
            lastDashTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
