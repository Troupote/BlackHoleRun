using UnityEngine;
using FMOD.Studio;
using DG.Tweening;
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

    private EventInstance footstepEvent; 
    private EventInstance ambienceEventInstance;

    
    private bool isFadingOut = false;
    private float fadeOutDuration = 0.5f;
    private Tween volumeFadeTween;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        footstepEvent = AudioManager.Instance.CreateEventInstance(FmodEventsCreator.instance.playerFootsetps);
        InitializeAmbience(FmodEventsCreator.instance.windAmbient);
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
    
    private void InitializeAmbience(EventReference ambienceEventRef)
    {
        ambienceEventInstance = AudioManager.Instance.CreateEventInstance(ambienceEventRef);
        AudioManager.Instance.Set3DAttributesFromTransform(ambienceEventInstance, transform);
        ambienceEventInstance.setVolume(AudioManager.Instance.AmbiantVolume);
        var result = ambienceEventInstance.start();
    }

	private void SetAmbienceParameter(string parameterName, float value)
    {
        ambienceEventInstance.setParameterByName(parameterName, value);
    }
    
    private void UpdateSound()
    {
        if ((Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) && isGrounded)
        {
            // Arrête le fade si on recommence à bouger
            if (isFadingOut && volumeFadeTween != null)
            {
                volumeFadeTween.Kill();
                isFadingOut = false;
            }
    
            PLAYBACK_STATE state;
            footstepEvent.getPlaybackState(out state);
            footstepEvent.setVolume(AudioManager.Instance.SFXVolume);
            AudioManager.Instance.Set3DAttributesFromTransform(footstepEvent, transform, currentVelocity);
    
            if (state.Equals(PLAYBACK_STATE.STOPPED))
            {
                footstepEvent.start();
            }
        }
        else if (!isFadingOut)
        {
            isFadingOut = true;
            
            float currentVolume;
            footstepEvent.getVolume(out currentVolume);
            
            volumeFadeTween = DOTween.To(
                () => currentVolume, 
                volume => footstepEvent.setVolume(volume),
                0f, 
                fadeOutDuration
            ).OnComplete(() => {
                footstepEvent.stop(STOP_MODE.ALLOWFADEOUT);
                isFadingOut = false;
            });
        }
    }
}

