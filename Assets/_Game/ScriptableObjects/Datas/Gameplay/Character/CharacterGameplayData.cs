using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "GameplayData", menuName = "Gameplay/CharacterGameplayData")]
public class CharacterGameplayData : ScriptableObject
{
    [Title("General Settings")]
    [SerializeField]
    private float m_cooldownBeforeThrowAllowed = 2f;

    public float CooldownBeforeThrowAllowed => m_cooldownBeforeThrowAllowed;

    [SerializeField]
    private float m_maxDistanceBetweenPlayers = 125f;

    public float MaxDistanceBetweenPlayers => m_maxDistanceBetweenPlayers;

    [Title("Singularity Settings")]

    [SerializeField]
    private float m_throwForce = 30f;
    public float ThrowForce => m_throwForce;

    [SerializeField]
    private float m_movingCurveForce = 15f;
    public float MovingCurveForce => m_movingCurveForce;

    [SerializeField] 
    private AnimationCurve m_joinBackToCharacterSpeed;

    public AnimationCurve JoinBackToCharacterSpeed => m_joinBackToCharacterSpeed;

    [SerializeField]
    private float m_DashForce = 40f;
    public float SingularityDashForce => m_DashForce;

    [SerializeField]
    private float m_JumpForce = 20f;
    public float SingularityJumpForce => m_JumpForce;

    [Title("Character Settings")]
    [SerializeField]
    private float m_playerSpeed = 30f;
    public float PlayerSpeed => m_playerSpeed;

    [SerializeField]
    private float m_jumpForce = 12f;
    public float JumpForce => m_jumpForce;

    [SerializeField]
    private float m_dashForce = 5f;
    public float DashForce => m_dashForce;

    [SerializeField]
    private float m_dashCooldown = 1.5f;
    public float DashCooldown => m_dashCooldown;

    [SerializeField]
    private float m_gravityScale = 8f;
    public float CharacterGravityScale => m_gravityScale;

    [Title("Camera Settings")]
    [SerializeField]
    private float m_baseFOV = 60f;
    public float BaseFOV => m_baseFOV;
    [SerializeField]
    private float m_MovingForwardFOV = 70f;
    public float MovingForwardFOV => m_MovingForwardFOV;
    [SerializeField]
    private float m_MovingBackwardFOV = 50f;
    public float MovingBackwardFOV => m_MovingBackwardFOV;
}
