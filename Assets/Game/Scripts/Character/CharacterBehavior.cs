using UnityEngine;

public class CharacterBehavior : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private bool _hasTouchedGround = true;

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
        _hasTouchedGround = false;
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
