using System.Runtime.CompilerServices;
using UnityEngine;

public class SingularityBehavior : MonoBehaviour
{
    [Header("-----Dependencies-----")]
    [SerializeField]
    private Camera _camera;

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
    private float _throwSpeed;

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
        Debug.Log("Throwing the ball");

        Vector3 throwDirection = _camera.transform.forward;

        _rigidbody.AddForce(throwDirection * _throwForce, ForceMode.Impulse);
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
    }
}
