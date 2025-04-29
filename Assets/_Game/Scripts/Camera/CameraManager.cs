using BHR;
using Cinemachine;
using System.Collections;
using UnityEngine;

public class CameraManager : ManagerSingleton<CameraManager>
{
    [field: SerializeField]
    internal CinemachineBrain MainCamBrain { get; private set; }

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

    void Start()
    {
        PlayerCam.Priority = 5;
        SingularityCam.Priority = 0;
        SingularityCam.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;

        PlayerCam.m_Lens.FieldOfView = CharactersManager.Instance.GameplayData.BaseFOV;
    }

    public void SetDependencies(GameObject a_characterToFollow, GameObject a_singularityToFollow)
    {
        PlayerCam.Follow = a_characterToFollow.transform;
        SingularityCam.Follow = a_singularityToFollow.transform;
    }

    private void OnEnable()
    {
        // Bind inputs
        PlayersInputManager.Instance.OnHLook.AddListener(HandleLook);
        PlayersInputManager.Instance.OnHMove.AddListener(HandlePlayerMove);
    }

    private void OnDisable()
    {
        // Debing inputs
        PlayersInputManager.Instance.OnHLook.RemoveListener(HandleLook);
        PlayersInputManager.Instance.OnHMove.RemoveListener(HandlePlayerMove);
    }

    internal bool IsBlending => MainCamBrain.IsBlending;

    public void SwitchCameraToSingularity()
    {
        initialRotationY = transform.eulerAngles.y;
        rotationY = initialRotationY;

        SingularityCam.transform.rotation = PlayerCam.transform.rotation;

        SingularityCam.gameObject.SetActive(true);
        PlayerCam.Priority = 0;
        SingularityCam.Priority = 5;
    }

    public void SwitchCameraToCharacter(Vector3 a_characterPosition)
    {
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


    void Update()
    {
        float moveZ = playerMoveValue.y;
        Vector2 sensitivity = currentControllerUsed == PlayerControllerState.KEYBOARD ? CharactersManager.Instance.GameplayData.MouseSensitivity : CharactersManager.Instance.GameplayData.GamepadSensitivity;
        sensitivity *= SettingsSave.LoadSensitivity(GameManager.Instance.ActivePlayerIndex);

        float mouseX = lookValue.x * sensitivity.x * Time.deltaTime;
        float mouseY = lookValue.y * sensitivity.y * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90, 90);

        if (CharactersManager.Instance.isSingularityThrown)
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
            PlayerCam.Follow.gameObject.transform.Rotate(Vector3.up * mouseX);
        }

        // Adjust FOV
        
        float currentFOV = PlayerCam.m_Lens.FieldOfView;

        if (moveZ > 0)
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
