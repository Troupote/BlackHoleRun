using Cinemachine;
using Unity.PlasticSCM.Editor.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using FMOD.Studio;
using FMODUnity;
using STOP_MODE = FMOD.Studio.STOP_MODE;


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
    private float footstepTimer = 0f;
    
    [SerializeField] private float footstepInterval = 0.3f;
    [SerializeField] private float minSpeedFactor = 0.5f;
    [SerializeField] private float maxSpeedFactor = 3f;
    [SerializeField] private float stepVolume = 1f;
    
    //audio 
    public EventInstance playerFootsteps;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerFootsteps = AudioManager.instance.CreateInstance(FMODEvent.instance.Footsteps);
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(moveX, 0, moveZ) * playerSpeed * Time.deltaTime;


        isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x + .3f);

        //Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown)
        {
            rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);

            lastDashTime = Time.time;
        }

        move = transform.TransformDirection(move);
        transform.localPosition += move;
        
        Debug.Log("justeavant le son ");
        currentVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
        
        UpdateSound();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    
    private void UpdateSound()
    {
        // Calculer la vitesse horizontale (ignorer le mouvement vertical)
        float horizontalSpeed = new Vector2(currentVelocity.x, currentVelocity.z).magnitude;
        bool isMoving = horizontalSpeed > 0.2f && isGrounded;

        if (isMoving && isGrounded)
        {
            footstepTimer += Time.deltaTime;
            
            float currentInterval = footstepInterval;
            

            if (footstepTimer >= currentInterval)
            {
                footstepTimer = 0f;

                var attributes = RuntimeUtils.To3DAttributes(transform.position);
                playerFootsteps.set3DAttributes(attributes);

                playerFootsteps.setVolume(stepVolume);

                PLAYBACK_STATE playbackState;
                playerFootsteps.getPlaybackState(out playbackState);
                if (playbackState != PLAYBACK_STATE.PLAYING)
                {
                    playerFootsteps.start();
                }
            }
        }
        else
        {
            
            playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            footstepTimer = footstepInterval; // Prêt pour le prochain pas
        }
    }
}