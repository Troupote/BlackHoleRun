using System.Runtime.CompilerServices;
using BHR;
using Cinemachine;
using UnityEngine;

public class SingularityBehavior : MonoBehaviour
{
    [Header("-----Dependencies-----")]
    [SerializeField] private CinemachineVirtualCamera cameraRef;

    private Rigidbody rb;
    private GameplayData gameplayData;

    private bool isThrown = false;
    private Vector3 followVelocity = Vector3.zero;

    public bool IsThrown => isThrown;
    public bool AlreadyCollided { get; private set; } = false;

    private void Start()
    {
        InitializeDependencies();
    }

    private void InitializeDependencies()
    {
        rb = GetComponent<Rigidbody>();
        cameraRef = CameraManager.Instance.PlayerCam;
        gameplayData = CharactersManager.Instance.GameplayData;
    }

    private void SetRigidbodyState(bool isActive)
    {
        rb.isKinematic = !isActive;
        rb.useGravity = isActive;

        if (isActive)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void FollowPlayer()
    {
        if (isThrown || rb == null) return;

        SetRigidbodyState(false);
        Transform followTarget = CameraManager.Instance.SingularityPlacementRefTransform;
        Vector3 targetPos = followTarget.position;
        Vector3 smoothedPos = Vector3.SmoothDamp(transform.position, targetPos, ref followVelocity, 0.1f);
        rb.MovePosition(smoothedPos);
    }

    public void Throw()
    {
        if (isThrown) return;

        isThrown = true;
        SetRigidbodyState(true);

        Vector3 forwardDirection = GetThrowDirection();
        ApplyThrowForce(forwardDirection);

        CameraManager.Instance.SwitchCameraToSingularity();
        CharactersManager.Instance.IsSingularityThrown(true);
        GameManager.Instance.ChangeMainPlayerState(PlayerState.SINGULARITY, true);
    }

    private Vector3 GetThrowDirection()
    {
        Vector3 playerVelocityXZ = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        return cameraRef.transform.forward + (playerVelocityXZ * 0.2f);
    }

    private void ApplyThrowForce(Vector3 direction)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(direction.normalized * gameplayData.ThrowForce, ForceMode.Impulse);
    }

    public void ResetVelocity()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isThrown || AlreadyCollided || collision == null) return;

        AlreadyCollided = true;
        CharactersManager.Instance.ChangePlayersTurn();
    }

    public void ResetThrowState(bool allowThrow)
    {
        isThrown = !allowThrow;
        AlreadyCollided = false;
        CharactersManager.Instance.IsSingularityThrown(false);
    }
}
