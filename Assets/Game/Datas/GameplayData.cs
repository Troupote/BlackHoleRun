using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "GameplayData", menuName = "Scriptable Objects/GameplayData")]
public class GameplayData : ScriptableObject
{
    [Title("Singularity Settings")]
    [SerializeField]
    private float m_secondsBeforeSpawningCharacterBackIfNoCollision = 5f;
    public float SecondsBeforeSpawningCharacterBackIfNoCollision => m_secondsBeforeSpawningCharacterBackIfNoCollision;

    [SerializeField]
    private float m_throwForce = 30f;
    public float ThrowForce => m_throwForce;

    [Title("Character Settings")]
    [SerializeField]
    private float m_playerSpeed = 5f;
    public float PlayerSpeed => m_playerSpeed;

    [SerializeField]
    private float m_jumpForce = 5f;
    public float JumpForce => m_jumpForce;

    [SerializeField]
    private float m_dashForce = 5f;
    public float DashForce => m_dashForce;

    [SerializeField]
    private float m_dashCooldown = 1.5f;
    public float DashCooldown => m_dashCooldown;
}
