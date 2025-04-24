using Cinemachine;
using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [field: SerializeField]
    internal CinemachineBrain MainCamBrain { get; private set; }

    [field: SerializeField]
    internal CinemachineVirtualCamera PlayerCam { get; private set; }

    [field: SerializeField]
    internal CinemachineVirtualCamera SingularityCam { get; private set; }

    [field: SerializeField]
    internal Transform SingularityPlacementRefTransform { get; private set; }

    public static CameraManager Instance { get; private set; }

    private float rotationX = 0f;
    private float rotationY = 0f;
    private float initialRotationY;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        PlayerCam.Priority = 5;
        SingularityCam.Priority = 0;
        SingularityCam.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SetDependencies(GameObject a_characterToFollow, GameObject a_singularityToFollow)
    {
        PlayerCam.Follow = a_characterToFollow.transform;
        SingularityCam.Follow = a_singularityToFollow.transform;
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
        float moveZ = Input.GetAxisRaw("Vertical");

        float mouseX = Input.GetAxis("Mouse X") * 200 * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * 300 * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90, 90);

        if (CharactersManager.Instance.SingularityThrown)
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
        /*
        if (moveZ > 0)
        {
            PlayerCam.m_Lens.FieldOfView = Mathf.Lerp(PlayerCam.m_Lens.FieldOfView, 90, Time.deltaTime * 2);
        }
        else if (moveZ < 0)
        {
            PlayerCam.m_Lens.FieldOfView = Mathf.Lerp(PlayerCam.m_Lens.FieldOfView, 50, Time.deltaTime * 2);
        }
        else
        {
            PlayerCam.m_Lens.FieldOfView = Mathf.Lerp(PlayerCam.m_Lens.FieldOfView, 60, Time.deltaTime * 2);
        }
        */
    }
}
