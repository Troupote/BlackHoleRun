using System.Runtime.CompilerServices;
using Cinemachine;
using UnityEngine;

public class SingularityBehavior : MonoBehaviour
{
    [Header("-----Dependencies-----")]
    [SerializeField]
    private CinemachineVirtualCamera _camera;

    private Rigidbody _rigidbody;
    [Header("-----Singularity Placement-----")]
    [field: SerializeField]
    private float _offsetX = 2.2f;

    [field: SerializeField]
    private float _offsetY = -1f;

    [field: SerializeField]
    private float _offsetZ = 4.5f;

    [Header("-----Singularity Throw Settings-----")]
    [field: SerializeField]
    private float _secondsBeforeSpawningCharacterBackIfNoCollision;

    [field: SerializeField]
    private float _throwForce = 10f;

    private bool _isThrown = false;
    public bool IsThrown
    {
        get { return _isThrown; }
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _camera = CameraManager.Instance.PlayerCam;
    }

    private Vector3 velocity = Vector3.zero;

    public void FollowPlayer()
    {
        if (_isThrown) return;

        if (_rigidbody == null) return;
        RigidbodyShouldBeEnabled(false);

        var GameObjectAsRef = CameraManager.Instance.SingularityPlacementRefTransform;

        transform.position = Vector3.SmoothDamp(transform.position, GameObjectAsRef.position, ref velocity, 0.1f);
    }


    public void Throw(Rigidbody rb)
    {
        if (_isThrown) return;

        _isThrown = true;
        RigidbodyShouldBeEnabled(true);

        Vector3 playerVelocity = rb != null ? rb.linearVelocity : Vector3.zero;

        Vector3 adjustedPlayerVelocity = new Vector3(playerVelocity.x, 0, playerVelocity.z);

        Vector3 throwDirection = _camera.transform.forward + (adjustedPlayerVelocity * 0.2f);

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _rigidbody.AddForce(throwDirection.normalized * _throwForce, ForceMode.Impulse);

        CameraManager.Instance.SwitchCameraToSingularity();
        CharactersManager.Instance.IsSingularityThrown(true);
    }

    void RigidbodyShouldBeEnabled(bool state)
    {
        _rigidbody.isKinematic = !state;
        _rigidbody.useGravity = state;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    internal bool AlreadyCollided { get; private set; } = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null || !_isThrown || AlreadyCollided) return;

        //_rigidbody.linearVelocity = Vector3.zero;
        //_rigidbody.angularVelocity = Vector3.zero;
        //RigidbodyShouldBeEnabled(false);

        AlreadyCollided = true;
        CharactersManager.Instance.ChangePlayersTurn(false);
    }

    public void ShouldAllowThrowAgain(bool a_shouldBe)
    {
        _isThrown = !a_shouldBe;
        AlreadyCollided = false;
        CharactersManager.Instance.IsSingularityThrown(false);
    }
}
