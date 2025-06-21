using BHR;
using Cinemachine;
using DG.Tweening;

//using DG.Tweening;
using System.Collections;
using UnityEngine;

public class CameraManager : ManagerSingleton<CameraManager>
{
    [field: SerializeField]
    internal Camera MainCam { get; private set; }

    private CinemachineBrain MainCamBrain;

    [field: SerializeField]
    internal CinemachineVirtualCamera PlayerCam { get; private set; }

    [field: SerializeField]
    internal CinemachineVirtualCamera SingularityCam { get; private set; }

    [field: SerializeField]
    internal Transform SingularityPlacementRefTransform { get; private set; }

    private CinemachineBasicMultiChannelPerlin m_playerCamNoise;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private float initialRotationY;
    private Vector2 lookValue;
    private Vector2 playerMoveValue;

    protected enum FOVState { NONE, BASE, FORWARD, BACK, AIM}
    private FOVState _fovState = FOVState.NONE;
    private Tween _fovTransitionTween = null;

    private PlayerControllerState currentControllerUsed = PlayerControllerState.DISCONNECTED;

    public CinemachineVirtualCamera CurrentCam => (PlayerCam.Priority > SingularityCam.Priority) ? PlayerCam : SingularityCam;

    public override void Awake()
    {
        SetInstance(false);
    }

    void Start()
    {
        MainCamBrain = MainCam.GetComponent<CinemachineBrain>();
        PlayerCam.Priority = 5;
        SingularityCam.Priority = 0;
        SingularityCam.gameObject.SetActive(false);
        m_playerCamNoise = PlayerCam.GetComponent<CinemachineBasicMultiChannelPerlin>();

        PlayerCam.m_Lens.FieldOfView = CharactersManager.Instance.GameplayData.BaseFOV;
    }

    private bool m_hasBeenInstancied = false;
    public void SetDependencies(GameObject a_characterToFollow, GameObject a_singularityToFollow)
    {
        PlayerCam.Follow = a_characterToFollow.transform;
        SingularityCam.Follow = a_singularityToFollow.transform;
        m_hasBeenInstancied = true;
    }

    private void OnEnable()
    {
        // Bind inputs
        PlayersInputManager.Instance.OnHLook.AddListener(HandleLook);
        PlayersInputManager.Instance.OnSLook.AddListener(HandleLook);
        PlayersInputManager.Instance.OnHMove.AddListener(HandlePlayerMove);

        GameManager.Instance.OnRespawned.AddListener(ResetInputs);
        GameManager.Instance.OnPaused.AddListener(ResetInputs);
    }

    private void OnDisable()
    {
        // Debing inputs
        PlayersInputManager.Instance.OnHLook.RemoveListener(HandleLook);
        PlayersInputManager.Instance.OnSLook.RemoveListener(HandleLook);
        PlayersInputManager.Instance.OnHMove.RemoveListener(HandlePlayerMove);

        GameManager.Instance.OnRespawned.RemoveListener(ResetInputs);
        GameManager.Instance.OnPaused.RemoveListener(ResetInputs);
    }

    internal bool IsBlending => MainCamBrain.IsBlending;

    private void ResetInputs()
    {
        lookValue = Vector2.zero;
        playerMoveValue = Vector2.zero;
    }

    public void ForceCameraLookAt(Vector3 targetLook)
    {
        targetLook.Normalize();
        PlayerCam.transform.localRotation = Quaternion.LookRotation(targetLook);
        rotationX = PlayerCam.transform.localRotation.eulerAngles.x;
        rotationY = PlayerCam.transform.localRotation.eulerAngles.y;
    }


    public void ForceSingularityCamLookAt()
    {
        PlayerCam.transform.rotation = Quaternion.Euler(lookValue);
        initialRotationY = PlayerCam.transform.eulerAngles.y;
        rotationY = initialRotationY;

        SingularityCam.transform.rotation = PlayerCam.transform.rotation;
    }

    public void SwitchCameraToSingularity()
    {
        if (CurrentCam == SingularityCam) return;

        Debug.Log("Switching camera to Singularity...");

        initialRotationY = PlayerCam.transform.eulerAngles.y;
        rotationY = initialRotationY;

        SingularityCam.transform.rotation = PlayerCam.transform.rotation;

        SingularityCam.gameObject.SetActive(true);
        PlayerCam.Priority = 0;
        SingularityCam.Priority = 5;
    }

    public void SwitchCameraToCharacter(Vector3 a_characterPosition)
    {
        if (CurrentCam == PlayerCam) return;

        PlayerCam.transform.position = a_characterPosition;
        PlayerCam.transform.rotation = SingularityCam.transform.rotation;

        StartCoroutine(InstantSwitch());
    }

    private IEnumerator InstantSwitch()
    {
        MainCamBrain.enabled = false;
        yield return new WaitForEndOfFrame();

        PlayerCam.Priority = 5;
        SingularityCam.Priority = 0;
        SingularityCam.gameObject.SetActive(false);

        MainCamBrain.enabled = true;
    }

    public void InstantBlendPlayerToSingularity()
    {
        StartCoroutine(InstantBlendPlayerToSingularityCoroutine());
    }

    private IEnumerator InstantBlendPlayerToSingularityCoroutine()
    {
        MainCamBrain.enabled = false;
        yield return new WaitForEndOfFrame();
        MainCamBrain.enabled = true;
    }

    void LateUpdate()
    {
        if (!m_hasBeenInstancied || !GameManager.Instance.IsPlaying) return;

        if (CharactersManager.Instance.IsDashing) return;

        if (CharactersManager.Instance.IsEndingCinematicStarted) return;

        float moveZ = playerMoveValue.y;
        Vector2 baseSensitivity = currentControllerUsed == PlayerControllerState.KEYBOARD ? SettingsManager.Instance.BaseSettings.MouseSensitivity : SettingsManager.Instance.BaseSettings.GamepadSensitivity;

        float mouseX = lookValue.x * baseSensitivity.x * SettingsSave.LoadSensitivityX(PlayersInputManager.Instance.CurrentActivePlayerDevice) * Time.deltaTime * GameManager.Instance.GameTimeScale;
        float mouseY = lookValue.y * baseSensitivity.y * SettingsSave.LoadSensitivityY(PlayersInputManager.Instance.CurrentActivePlayerDevice) * Time.deltaTime * GameManager.Instance.GameTimeScale;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -89, 89);

        if (CurrentCam == SingularityCam)
        {
            float targetRotationY = rotationY + mouseX;
            targetRotationY = Mathf.Clamp(targetRotationY, initialRotationY - 90, initialRotationY + 90);
            rotationY = targetRotationY;

            SingularityCam.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        }
        else
        {
            rotationY += mouseX;
            PlayerCam.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
            //PlayerCam.Follow.gameObject.transform.Rotate(Vector3.up * mouseX);
        }

        // Adjust FOV
        
        float currentFOV = PlayerCam.m_Lens.FieldOfView;

        if (CharactersManager.Instance.isHumanoidAiming)
            ChangeFOVState(FOVState.AIM);
        else if (moveZ > 0)
            ChangeFOVState(FOVState.FORWARD);
        else if (moveZ < 0)
            ChangeFOVState(FOVState.BACK);
        else
            ChangeFOVState(FOVState.BASE);
        
    }

    private void ChangeFOVState(FOVState newState)
    {
        if (newState == _fovState) return;

        CharacterGameplayData data = CharactersManager.Instance.GameplayData;

        // Time transition
        float tweenDuration = (newState == FOVState.AIM || _fovState == FOVState.AIM) ? data.TriggerAimDuration : data.BaseFOVTransitionDuration;

        // Set ease
        Ease ease = (newState == FOVState.AIM || _fovState == FOVState.AIM) ? data.AimFOVTransitionEase : data.DefaultFOVTransitionEase;

        // Choose target FOV
        float targetFOV = newState switch
        {
            FOVState.BASE => data.BaseFOV,
            FOVState.FORWARD => data.MovingForwardFOV,
            FOVState.BACK => data.MovingBackwardFOV,
            FOVState.AIM => data.TargetAimFOV,
            _ => data.BaseFOV
        };

        // Launch transition (and kill if there is one at the moment)
        if (_fovTransitionTween != null)
            _fovTransitionTween.Kill();

        _fovTransitionTween = DOTween.To(() => PlayerCam.m_Lens.FieldOfView, x => PlayerCam.m_Lens.FieldOfView = x, targetFOV, tweenDuration).SetEase(ease);

        //Debug.Log($"From {_fovState} to {newState}, with new FOV set at {targetFOV} in {tweenDuration} seconds");

        _fovState = newState;
    }

    public void HandleLook(Vector2 value, PlayerControllerState controller)
    {
        currentControllerUsed = controller;
        lookValue = value;
    }
    public void HandlePlayerMove(Vector2 value) => playerMoveValue = value;

    public void ShakeCamera(float intensity, float duration, float frequency = 2f)
    {
        CinemachineVirtualCamera camToShake = CurrentCam;

        var noise = camToShake.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogWarning("CinemachineBasicMultiChannelPerlin not found on active virtual camera.");
            return;
        }

        noise.m_AmplitudeGain = intensity;
        noise.m_FrequencyGain = frequency;

        StartCoroutine(ResetNoiseAfterDelay(noise, duration));
    }

    private IEnumerator ResetNoiseAfterDelay(CinemachineBasicMultiChannelPerlin noise, float delay)
    {
        yield return new WaitForSeconds(delay);

        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
    }


}
