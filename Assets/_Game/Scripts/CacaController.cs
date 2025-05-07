using UnityEngine;
using FMOD.Studio;

public class CacaController : MonoBehaviour
{
    public float playerSpeed = 5;
    public float jumpForce = 5;
    public float dashForce = 5;

    private float dashCooldown = 1.5f;
    private float lastDashTime = 0;

    private Rigidbody rb;
    private bool isGrounded;

    public GameObject blackHole;
    public GameObject destination;
    
    private Vector3 lastPosition;
    private Vector3 currentVelocity;

    private EventInstance footstepEvent; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        footstepEvent = AudioManager.Instance.CreateEventInstance(FmodEventsCreator.instance.playerFootsetps);
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(moveX, 0, moveZ) * playerSpeed * Time.deltaTime;


        isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x + .3f);
        
        Debug.DrawRay(transform.position, Vector3.down * (transform.localScale.x + .3f), Color.red);
        isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x + .3f);

        //Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown)
        {
            rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);

            lastDashTime = Time.time;
        }
        
        UpdateSound();
        
        move = transform.TransformDirection(move);
        transform.localPosition += move;
        
        currentVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
        

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void UpdateSound()
    {
        if ((Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) && isGrounded)
        {
            PLAYBACK_STATE state;

            footstepEvent.getPlaybackState(out state);

            footstepEvent.setVolume(AudioManager.Instance.SFXVolume);

            AudioManager.Instance.Set3DAttributesFromTransform(footstepEvent, transform, currentVelocity);

            if (state.Equals(PLAYBACK_STATE.STOPPED))
            {
                footstepEvent.start();
            }
        }
        else
        {
            footstepEvent.stop(STOP_MODE.ALLOWFADEOUT);
        }
        

    }
}