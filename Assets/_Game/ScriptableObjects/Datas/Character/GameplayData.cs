using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "GameplayData", menuName = "Scriptable Objects/GameplayData")]
public class GameplayData : ScriptableObject
{
    [Title("Singularity Settings")]
    [SerializeField]
    private float m_secondsBeforeSpawningCharacterBackIfNoCollision = 4f;
    public float SecondsBeforeSpawningCharacterBackIfNoCollision => m_secondsBeforeSpawningCharacterBackIfNoCollision;

    [SerializeField]
    private float m_throwForce = 30f;
    public float ThrowForce => m_throwForce;

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
