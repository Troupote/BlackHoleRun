using Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [field: SerializeField]
    internal CinemachineVirtualCamera PlayerCam { get; private set; }
    [field: SerializeField]
    internal CinemachineVirtualCamera SingularityCam { get; private set; }

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
        PlayerCam.Priority = 5;
        SingularityCam.Priority = 0;
    }

    public void SetDependencies(GameObject a_characterToFollow, GameObject a_singularityToFollow)
    {
        PlayerCam.Follow = a_characterToFollow.transform;
        SingularityCam.Follow = a_singularityToFollow.transform;
    }

    public void SwitchCameraToSingularity()
    {
        PlayerCam.Priority = 0;
        SingularityCam.Priority = 5;
    }

    public void SwitchCameraToCharacter()
    {
        PlayerCam.Priority = 5;
        SingularityCam.Priority = 0; 
    }
}
