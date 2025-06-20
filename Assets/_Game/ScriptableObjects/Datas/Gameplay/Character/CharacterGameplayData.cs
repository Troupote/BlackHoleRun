using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using DG.Tweening;

[CreateAssetMenu(fileName = "GameplayData", menuName = "Gameplay/CharacterGameplayData")]
public class CharacterGameplayData : ScriptableObject
{
    [Title("General Settings")]
    //[SerializeField, PropertyOrder(0)]
    //private float m_cooldownBeforeThrowAllowed = 2f;

    //public float CooldownBeforeThrowAllowed => m_cooldownBeforeThrowAllowed;

    [SerializeField,PropertyOrder(1)]
    AnimationCurve _comingBackCurve = AnimationCurve.Linear(0,0,100,100);
    public AnimationCurve ComingBackCurve => _comingBackCurve;
    [Button, PropertyOrder(2)]
    private void InitCurve(float timeStart, float startValue, float endTime, float endValue) => _comingBackCurve = AnimationCurve.Linear(timeStart, startValue, endTime, endValue);

    [SerializeField, PropertyOrder(3)]
    private float m_maxDistanceBetweenPlayers = 125f;

    public float MaxDistanceBetweenPlayers => m_maxDistanceBetweenPlayers;

    [SerializeField, PropertyOrder(3)]
    private bool m_activateMovementsLimit = false;
    public bool ActivateMovementsLimit => m_activateMovementsLimit;

    [Title("Singularity Settings")]

    [SerializeField, PropertyOrder(3)]
    private float m_throwForce = 30f;
    public float ThrowForce => m_throwForce;

    [SerializeField, PropertyOrder(3)]
    private float m_movingCurveForce = 15f;
    public float MovingCurveForce => m_movingCurveForce;

    [SerializeField, PropertyOrder(3)]
    private float m_DashForce = 40f;
    public float SingularityDashForce => m_DashForce;

    [SerializeField, PropertyOrder(3)]
    private float m_JumpForce = 20f;
    public float SingularityJumpForce => m_JumpForce;

    [SerializeField, PropertyOrder(3)]
    private float m_throwingCenterDistanceMultiplier = 10f;
    public float ThrowingCenterDistanceMultiplier => m_throwingCenterDistanceMultiplier;

    [SerializeField, PropertyOrder(3)]
    private float m_timeTakenToGoToCenter = 0.1f;
    public float TimeTakenToGoToCenter => m_timeTakenToGoToCenter;

    [SerializeField, PropertyOrder(3)]
    [Tooltip("The delay before the singularity starts falling after being thrown.")]
    private float m_gravityDelayDuration = 1f;
    public float GravityDelayDuration => m_gravityDelayDuration;

    [SerializeField, PropertyOrder(3)]
    private float m_gravityMultiplier = 2f;
    public float GravityMultiplier => m_gravityMultiplier;

    [Title("Character Settings")]
    [SerializeField, PropertyOrder(3)]
    private float m_playerSpeed = 30f;
    public float PlayerSpeed => m_playerSpeed;

    [SerializeField, PropertyOrder(3)]
    private float m_jumpForce = 8f;
    public float JumpForce => m_jumpForce;

    [SerializeField, PropertyOrder(3)]
    private float m_coyotteTime = 0.15f;
    public float CoyotteTime => m_coyotteTime;

    [SerializeField, PropertyOrder(3)]
    private float m_dashForce = 1f;
    public float DashForce => m_dashForce;

    [SerializeField, PropertyOrder(3)]
    private float m_dashDuration = 1f;
    public float DashDuration => m_dashDuration;

    [SerializeField, PropertyOrder(3)]
    private float m_dashCooldown = 1.5f;
    public float DashCooldown => m_dashCooldown;

    [SerializeField, PropertyOrder(3)]
    private float m_gravityScale = 8f;
    public float CharacterGravityScale => m_gravityScale;

    [SerializeField, PropertyOrder(3)]
    private float m_airPlayerSpeedMultiplier = 0.5f;
    public float AirPlayerSpeedMultiplier => m_airPlayerSpeedMultiplier;

    [SerializeField, PropertyOrder(3)]
    private float m_triggerAimDuration;
    public float TriggerAimDuration => m_triggerAimDuration;

    [SerializeField, PropertyOrder(3)]
    private float m_targetAimTimeScale;
    public float TargetAimTimeScale => m_targetAimTimeScale;

    [SerializeField, PropertyOrder(3)]
    [Tooltip("The layer used for ground to check if the character is grounded using a raycast/checkSphere.")]
    private LayerMask m_groundMask;
    public LayerMask GroundMask => m_groundMask;

    [SerializeField, PropertyOrder(3)]
    [Tooltip("The distance from the ground at which the character is considered grounded for the CAPSULE COLLIDER (red gizmo)")]
    private float m_capsuleGroundDistance = 0.4f;
    public float CapsuleGroundDistance => m_capsuleGroundDistance;

    [SerializeField]
    [Tooltip("The distance from the ground at which the character is considered grounded for the RAYCAST (green gizmo)"), PropertyOrder(3)]
    private float m_raycastGroundDistance = 2f;
    public float RaycastGroundDistance => m_raycastGroundDistance;

    [Title("Camera Settings")]
    [SerializeField, PropertyOrder(3)]
    private float m_baseFOV = 60f;
    public float BaseFOV => m_baseFOV;

    [SerializeField, PropertyOrder(3)]
    private float m_baseFOVTransitionDuration = 2f;
    public float BaseFOVTransitionDuration => m_baseFOVTransitionDuration;
    [SerializeField, PropertyOrder(3)]
    private float m_MovingForwardFOV = 70f;
    public float MovingForwardFOV => m_MovingForwardFOV;
    [SerializeField, PropertyOrder(3)]
    private float m_MovingBackwardFOV = 50f;
    public float MovingBackwardFOV => m_MovingBackwardFOV;

    [SerializeField, PropertyOrder(3)]
    private float m_targetAimFOV;
    public float TargetAimFOV => m_targetAimFOV;

    [SerializeField, PropertyOrder(3)]
    private Ease _defaultFOVTransitionEase = Ease.Unset;
    public Ease DefaultFOVTransitionEase => _defaultFOVTransitionEase;
    [SerializeField, PropertyOrder(3)]
    private Ease _aimFOVTransitionEase = Ease.InOutCubic;
    public Ease AimFOVTransitionEase => _aimFOVTransitionEase;



    [System.Serializable]
    public class PlayerColors
    {
        public Color Exterior;
        public Color Interior;
    }

    [Title("Players Colors")]
    [SerializeField, PropertyOrder(3)]
    private PlayerColors m_playerOneColors;

    public PlayerColors PlayerOneColors => m_playerOneColors;

    [SerializeField, PropertyOrder(3)]
    private PlayerColors m_playerTwoColors;
    public PlayerColors PlayerTwoColors => m_playerTwoColors;

}
