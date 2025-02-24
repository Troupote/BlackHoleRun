using System.Runtime.CompilerServices;
using UnityEngine;

public class SingularityBehavior : MonoBehaviour
{
    [Header("-----Dependencies-----")]
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private Transform _playerTransform;

    private Rigidbody _rigidbody;
    [Header("-----Singularity Placement-----")]
    [field: SerializeField]
    private float _offsetX = 2.55f;

    [field: SerializeField]
    private float _offsetY = 0.5f;

    [field: SerializeField]
    private float _offsetZ = 5f;

    [Header("-----Singularity Throw Settings-----")]
    [field: SerializeField]
    private float _secondsBeforeSpawningCharacterBackIfNoCollision;

    [field: SerializeField]
    private float _throwForce = 10f;

    private bool _isThrown = false;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void FollowPlayer(Vector3 position)
    {
        if (_isThrown) return;

        Vector3 newPos = new Vector3(position.x + _offsetX, position.y + _offsetY, position.z + _offsetZ);
        transform.position = newPos;
    }

    public void Throw()
    {
        if (_isThrown) return;

        _isThrown = true;
        RigidbodyShouldBeEnabled(true);

        Vector3 throwDirection = _camera.transform.forward;

        _rigidbody.AddForce(throwDirection * _throwForce, ForceMode.Impulse);

        CameraSwitcher.Instance.SwitchCameraToSingularity();
    }

    private void RigidbodyShouldBeEnabled(bool a_shouldBe)
    {
        _rigidbody.useGravity = a_shouldBe;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        RigidbodyShouldBeEnabled(false);

        SwitchBackToCharacter();
    }

    private void SwitchBackToCharacter()
    {
        Debug.Log("Yes");
        _playerTransform.position = transform.position + new Vector3(0, 4, 0);
        CameraSwitcher.Instance.SwitchCameraToCharacter();
        _isThrown = false;
    }

    private float _timeElasped;
    private void Update()
    {
        if (!_isThrown)
        {
            _timeElasped = 0;
            return;
        }

        _timeElasped += Time.deltaTime;

        if (_timeElasped >= 4f)
        {
            SwitchBackToCharacter();
            _timeElasped = 0;
        }
    }
}
