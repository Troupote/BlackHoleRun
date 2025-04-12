using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterBehavior : MonoBehaviour
{
    [SerializeField]
    private SingularityBehavior _singularity;

    private Rigidbody _rigidbody;

    internal bool IsThrown => _singularity.IsThrown;

    private bool _hasTouchedGround = true;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void SetDependencies(SingularityBehavior a_singularity)
    {
        _singularity = a_singularity;
    }

    public void ThrowSingularity()
    {
        if (_singularity.IsThrown && !_singularity.AlreadyCollided)
        {
            CharactersManager.Instance.ChangePlayersTurn(true);
            return;
        }
        if (!_hasTouchedGround) return;

        _singularity.Throw(_rigidbody);
        _hasTouchedGround = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 6) return;

        _hasTouchedGround = true;
    }
}
