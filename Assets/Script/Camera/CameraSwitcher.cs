using Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [field: SerializeField]
    internal CinemachineBrain MainCamBrain { get; private set; }

    [field: SerializeField]
    internal CinemachineVirtualCamera PlayerCam { get; private set; }

    [field: SerializeField]
    internal CinemachineVirtualCamera SingularityCam { get; private set; }

    [field: SerializeField]
    internal Transform SingularityPlacementRefTransform{ get; private set; }

    public static CameraSwitcher Instance { get; private set; }

    private float rotationX = 0f;
    private float rotationY = 0f;

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
        SingularityCam.gameObject.SetActive(true);
        PlayerCam.Priority = 0;
        SingularityCam.Priority = 5;
    }

    public void SwitchCameraToCharacter()
    {
        SingularityCam.gameObject.SetActive(false);
        PlayerCam.Priority = 5;
        SingularityCam.Priority = 0; 
    }

    void Update()
    {
        float moveZ = Input.GetAxisRaw("Vertical");

        float mouseX = Input.GetAxis("Mouse X") * 200 * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * 300 * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90, 90);

        PlayerCam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.Rotate(Vector3.up * mouseX);
        PlayerCam.Follow.gameObject.transform.Rotate(Vector3.up * mouseX);

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
    }
}
