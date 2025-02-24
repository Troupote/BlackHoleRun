using Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera _playerCam;
    [SerializeField]
    private CinemachineVirtualCamera _singularityCam;

    public static CameraSwitcher Instance { get; private set; }

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
        _playerCam.Priority = 5;
        _singularityCam.Priority = 0;
    }

    public void SwitchCameraToSingularity()
    {
        _playerCam.Priority = 0;
        _singularityCam.Priority = 5;
    }

    public void SwitchCameraToCharacter()
    {
        _playerCam.Priority = 5;
        _singularityCam.Priority = 0; 
    }
}
