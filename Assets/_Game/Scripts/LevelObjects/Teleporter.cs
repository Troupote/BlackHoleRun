using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField]
    private Transform m_targetDestination;

    private enum TeleporterType
    {
        Player,
        Singularity,
        Both
    }

    [SerializeField]
    [Tooltip("Type of teleporter: Player, Singularity, or Both")]
    private TeleporterType m_teleporterType = TeleporterType.Singularity;

    [SerializeField]
    private float m_ZvelocityApplied = 20f;

    private bool IsGoodType(string a_gameobjectTag)
    {
        switch (m_teleporterType)
        {
            case TeleporterType.Player:
                return a_gameobjectTag == "Player";
            case TeleporterType.Singularity:
                return a_gameobjectTag == "Singularity" && !CharactersManager.Instance.CanThrow;
            case TeleporterType.Both:
                return a_gameobjectTag == "Player" || (a_gameobjectTag == "Singularity" && !CharactersManager.Instance.CanThrow);
            default:
                return false;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (IsGoodType(collision.gameObject.tag))
        {
            var rb = collision.gameObject.GetComponent<Rigidbody>();

            Vector3 newVelocity = m_targetDestination.forward.normalized;
            newVelocity.z = m_ZvelocityApplied;

            rb.position = m_targetDestination.position;

            rb.linearVelocity = newVelocity;

            CameraManager.Instance.InstantBlendPlayerToSingularity();
            CameraManager.Instance.ForceSingularityCamLookAt();
            Debug.Log("Teleporting");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_targetDestination == null)
            return;

        Gizmos.color = Color.cyan;

        Vector3 direction = m_targetDestination.forward;
        float arrowLength = 2f;

        Gizmos.DrawLine(m_targetDestination.position, m_targetDestination.position + direction * arrowLength);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -150, 0) * Vector3.forward;

        Gizmos.DrawLine(m_targetDestination.position + direction * arrowLength, m_targetDestination.position + direction * arrowLength + right * 0.3f);
        Gizmos.DrawLine(m_targetDestination.position + direction * arrowLength, m_targetDestination.position + direction * arrowLength + left * 0.3f);
    }
#endif
}
