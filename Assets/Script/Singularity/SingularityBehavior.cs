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
        _camera = CameraSwitcher.Instance.PlayerCam;
    }

    public void FollowPlayer(Vector3 position)
    {
        if (_isThrown) return;

        transform.SetParent(CameraSwitcher.Instance.PlayerCam.transform);

        Vector3 newPos = new Vector3(_offsetX, _offsetY, _offsetZ);
        transform.localPosition = newPos;
        transform.localRotation = Quaternion.identity;

        if (transform.position.y < 0.5f)
        {
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
        }
    }

    public void Throw(Rigidbody rb)
    {
        if (_isThrown) return;

        transform.SetParent(null);
        _isThrown = true;
        RigidbodyShouldBeEnabled(true);

        Vector3 playerVelocity = rb != null ? rb.linearVelocity : Vector3.zero;

        Vector3 adjustedPlayerVelocity = new Vector3(playerVelocity.x, 0, playerVelocity.z);

        Vector3 throwDirection = _camera.transform.forward + (adjustedPlayerVelocity * 0.2f);

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _rigidbody.AddForce(throwDirection.normalized * _throwForce, ForceMode.Impulse);

        CameraSwitcher.Instance.SwitchCameraToSingularity();
    }

    private void RigidbodyShouldBeEnabled(bool a_shouldBe)
    {
        _rigidbody.useGravity = a_shouldBe;
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
    }
}
