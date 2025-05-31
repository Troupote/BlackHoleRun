using BHR;
using Cinemachine;
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

    private float rotationX = 0f;
    private float rotationY = 0f;
    private float initialRotationY;
    private Vector2 lookValue;
    private Vector2 playerMoveValue;

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

        GameManager.Instance.OnRespawn.AddListener(ResetInputs);
        GameManager.Instance.OnPaused.AddListener(ResetInputs);
    }

    private void OnDisable()
    {
        // Debing inputs
        PlayersInputManager.Instance.OnHLook.RemoveListener(HandleLook);
        PlayersInputManager.Instance.OnSLook.RemoveListener(HandleLook);
        PlayersInputManager.Instance.OnHMove.RemoveListener(HandlePlayerMove);

        GameManager.Instance.OnRespawn.RemoveListener(ResetInputs);
        GameManager.Instance.OnPaused.RemoveListener(ResetInputs);
    }

    internal bool IsBlending => MainCamBrain.IsBlending;

    private void ResetInputs()
    {
        lookValue = Vector2.zero;
        playerMoveValue = Vector2.zero;
    }

    public void SwitchCameraToSingularity()
    {
        if (CurrentCam == SingularityCam) return;

        initialRotationY = transform.eulerAngles.y;
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

    void LateUpdate()
    {
        if (!m_hasBeenInstancied || !GameManager.Instance.IsPlaying) return;

        float moveZ = playerMoveValue.y;
        Vector2 baseSensitivity = currentControllerUsed == PlayerControllerState.KEYBOARD ? SettingsManager.Instance.BaseSettings.MouseSensitivity : SettingsManager.Instance.BaseSettings.GamepadSensitivity;

        float mouseX = lookValue.x * baseSensitivity.x * SettingsSave.LoadSensitivityX(PlayersInputManager.Instance.CurrentActivePlayerDevice) * Time.deltaTime * GameManager.Instance.GameTimeScale;
        float mouseY = lookValue.y * baseSensitivity.y * SettingsSave.LoadSensitivityY(PlayersInputManager.Instance.CurrentActivePlayerDevice) * Time.deltaTime * GameManager.Instance.GameTimeScale;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90, 90);

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
            transform.rotation = Quaternion.Euler(0, rotationY, 0);
            PlayerCam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            //PlayerCam.Follow.gameObject.transform.Rotate(Vector3.up * mouseX);
        }

        // Adjust FOV
        
        float currentFOV = PlayerCam.m_Lens.FieldOfView;

        if(CharactersManager.Instance.isHumanoidAiming)
        {
            PlayerCam.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, CharactersManager.Instance.GameplayData.TargetAimFOV, (Time.deltaTime * 3) / CharactersManager.Instance.GameplayData.TriggerAimDuration);
            //DOTween.To(() => PlayerCam.m_Lens.FieldOfView, x => PlayerCam.m_Lens.FieldOfView = x, CharactersManager.Instance.GameplayData.TargetAimFOV, CharactersManager.Instance.GameplayData.TriggerAimDuration);
        }
        else if (moveZ > 0)
        {
            PlayerCam.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, CharactersManager.Instance.GameplayData.MovingForwardFOV, Time.deltaTime * 2);
        }
        else if (moveZ < 0)
        {
            PlayerCam.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, CharactersManager.Instance.GameplayData.MovingBackwardFOV, Time.deltaTime * 2);
        }
        else
        {
            PlayerCam.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, CharactersManager.Instance.GameplayData.BaseFOV, Time.deltaTime * 2);
        }
        
    }

    public void HandleLook(Vector2 value, PlayerControllerState controller)
    {
        currentControllerUsed = controller;
        lookValue = value;
    }
    public void HandlePlayerMove(Vector2 value) => playerMoveValue = value;
}
