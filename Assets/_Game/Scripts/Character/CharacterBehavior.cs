using UnityEngine;

public class CharacterBehavior : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private bool _hasTouchedGround = true;
    public bool HasTouchedGround => _hasTouchedGround;

    [SerializeField]
    private LayerMask m_groundLayer;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void TryThrowSingularity()
    {
        if (!_hasTouchedGround) return;

        CharactersManager.Instance.TryThrowSingularity();
    }

    public void ResetVelocity()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void ImobilizeCharacter(bool a_shouldImobilize)
    {
        _rigidbody.useGravity = !a_shouldImobilize;
        _rigidbody.isKinematic = a_shouldImobilize;

        if (a_shouldImobilize)
        { 
            _hasTouchedGround = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object we collided with is part of the ground layer
        if (((1 << collision.gameObject.layer) & m_groundLayer) != 0)
        {
            _hasTouchedGround = true;
        }
    }
}
