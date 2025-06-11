using BHR;
using System;
using System.Collections;
using UnityEngine;

public class CharactersManager : ManagerSingleton<CharactersManager>
{
    [SerializeField]
    private GameObject m_characterPrefab;

    [SerializeField]
    private GameObject m_singularityPrefab;

    [SerializeField]
    private CharacterGameplayData m_gameplayData;

    internal CharacterGameplayData GameplayData => m_gameplayData;

    [field: SerializeField]
    internal LimitPlayersMovementsController LimitPlayersMovements { get; private set; } = null;

    [field: SerializeField]
    internal NewSingularityPreviewController SingularityPreviewController { get; private set; } = null;

    [SerializeField]
    private LayerMask groundLayer;

    private GameObject m_singularityObject;
    private GameObject m_characterObject;
    public GameObject CharacterObject => m_characterObject;

    private SingularityBehavior m_singularityBehavior;
    private CharacterBehavior m_characterBehavior;

    public Action ResetInputs;

    public bool isHumanoidAiming = false;

    internal bool CanThrow => m_singularityBehavior.IsAllowedToBeThrown;

    public override void Awake()
    {
        SetInstance(false);
    }

    private void Start()
    {
        SpawnCharacterAtPosition(CheckpointsManager.Instance.CurrentCheckpoint.position);
    }

    private bool m_isInstancied = false;
    private void InstanciatePrefabsOnScene()
    {
        m_characterObject = Instantiate(m_characterPrefab);
        m_singularityObject = Instantiate(m_singularityPrefab);

        m_singularityBehavior = m_singularityObject.GetComponent<SingularityBehavior>();
        m_characterBehavior = m_characterObject.GetComponent<CharacterBehavior>();

        CameraManager.Instance.SetDependencies(m_characterObject, m_singularityObject);
        m_characterBehavior.InitializeDependencies(m_gameplayData);
        m_singularityBehavior.InitializeDependencies(m_gameplayData);

        ListenToEvents();

        m_isInstancied = true;
    }

    private void ListenToEvents()
    {
        m_characterBehavior.OnThrowInput += ThrowSingularity;
        m_singularityBehavior.OnThrowPerformed += OnThrowPerformed;
        m_singularityBehavior.OnUnmorph += SwitchCharactersPositions;
        m_singularityBehavior.OnJump += OnSingularityJump;
        m_singularityBehavior.OnDash += SingularityDash;

        GameManager.Instance.OnRespawned.AddListener(HardReset);
    }

    private void UnlistenToEvents()
    {
        m_singularityBehavior.OnThrowPerformed -= OnThrowPerformed;
        m_characterBehavior.OnThrowInput -= ThrowSingularity;
        m_singularityBehavior.OnUnmorph -= SwitchCharactersPositions;
        m_singularityBehavior.OnJump -= OnSingularityJump;
        m_singularityBehavior.OnDash -= SingularityDash;

        GameManager.Instance.OnRespawned.RemoveListener(HardReset);
    }

    private bool AreObjectsInstancied() => m_isInstancied;

    public void SpawnCharacterAtPosition(Vector3 a_position)
    {
        if (!AreObjectsInstancied())
        {
            InstanciatePrefabsOnScene();
        }

        m_characterObject.transform.position = a_position;
        m_singularityBehavior.SingularityCharacterFollowComponent.PickupSingularity(true);
    }

    public void HardReset()
    {
        Debug.Log("Hard Reset Characters Manager");

        if (BringNewSingularityToNewCharacterCoroutine != null)
        {
            StopCoroutine(BringNewSingularityToNewCharacterCoroutine);
            BringNewSingularityToNewCharacterCoroutine = null;
            m_isSwitching = false;
            m_singularityBehavior.SetIgnoreCollision(false);
        }

        var rbCharacter = m_characterObject.GetComponent<Rigidbody>();
        rbCharacter.linearVelocity = Vector3.zero;
        rbCharacter.angularVelocity = Vector3.zero;

        var rbSingularity = m_singularityObject.GetComponent<Rigidbody>();
        rbSingularity.linearVelocity = Vector3.zero;
        rbSingularity.angularVelocity = Vector3.zero;

        m_singularityBehavior.SingularityCharacterFollowComponent.PickupSingularity(true);

        CameraManager.Instance.SwitchCameraToCharacter(m_characterObject.transform.position);
        GameManager.Instance.ChangeMainPlayerState(PlayerState.HUMANOID, false);
        m_characterBehavior.ImobilizeCharacter(false);

        ResetInputs?.Invoke();

        if (m_gameplayData.ActivateMovementsLimit)
        {
            LimitPlayersMovements.ClearPerformedMovements();
        }
    }


    private bool IsDistanceBetweenPlayersExceeded()
    {
        return (Vector3.Distance(m_characterObject.transform.position, m_singularityObject.transform.position)
            > m_gameplayData.MaxDistanceBetweenPlayers);
    }

    #region Life Cycle

    private void FixedUpdate()
    {
        if (!AreObjectsInstancied()) return;

    }

    private void Update()
    {
        if (!AreObjectsInstancied()) return;

        // Check if the distance between players is exceeded
        if (IsDistanceBetweenPlayersExceeded() && !m_singularityBehavior.SingularityCharacterFollowComponent.IsPickedUp)
        {
            SwitchCharactersPositions();
        }
    }


    #endregion

    #region Switch Characters

    private Coroutine BringNewSingularityToNewCharacterCoroutine;
    float GetCharacterHeight()
    {
        CapsuleCollider collider = m_characterObject.GetComponent<CapsuleCollider>();
        return collider ? collider.height : 2.0f;
    }

    private bool m_isSwitching = false;
    public bool IsCurrentlySwitching => m_isSwitching;
    public void SwitchCharactersPositions()
    {
        if (m_isSwitching) return;

        m_isSwitching = true;

        HideSingularityPreview();

        Vector3 singularityPosition = m_singularityObject.transform.position;
        var oldCharacterPosition = m_characterObject.transform.position;

        RaycastHit hit;
        // To make sure the character does not get stuck underground
        if (Physics.Raycast(singularityPosition + Vector3.up * 0.5f, Vector3.down, out hit, 2.0f, groundLayer))
        {
            singularityPosition.y = hit.point.y + GetCharacterHeight();
        }

        CorrectlySwitchPositionsOfPlayers(singularityPosition, oldCharacterPosition);

        CameraManager.Instance.SwitchCameraToCharacter(m_characterObject.transform.position);

        ResetInputs?.Invoke();

        GameManager.Instance.ChangeMainPlayerState(PlayerState.HUMANOID, false);

        if (m_gameplayData.ActivateMovementsLimit)
            LimitPlayersMovements.ClearPerformedMovements();

        m_characterBehavior.ImobilizeCharacter(false);

        BringNewSingularityToNewCharacterCoroutine = StartCoroutine(MoveSlowlySingularityToNewCharacter(() =>
        {
            m_singularityBehavior.SingularityCharacterFollowComponent.PickupSingularity(true);
        }));
    }

    private void CorrectlySwitchPositionsOfPlayers(Vector3 a_singularityPosition, Vector3 a_oldCharacterPosition)
    {
        var rbCharacter = m_characterObject.GetComponent<Rigidbody>();
        var rbSingularity = m_singularityObject.GetComponent<Rigidbody>();

        var interpolationCharacter = rbCharacter.interpolation;
        var interpolationSingularity = rbSingularity.interpolation;

        rbCharacter.interpolation = RigidbodyInterpolation.None;
        rbSingularity.interpolation = RigidbodyInterpolation.None;

        rbCharacter.position = a_singularityPosition;
        rbSingularity.position = a_oldCharacterPosition;

        m_characterObject.transform.position = a_singularityPosition;
        m_singularityObject.transform.position = a_oldCharacterPosition;

        rbCharacter.interpolation = interpolationCharacter;
        rbSingularity.interpolation = interpolationSingularity;

        rbSingularity.isKinematic = true;

        SingularityPreviewController.Deflate();
    }
    private IEnumerator MoveSlowlySingularityToNewCharacter(Action onComplete = null)
    {
        m_singularityBehavior.SetIgnoreCollision(true);

        yield return WaitForCustomSeconds(m_gameplayData.CooldownBeforeThrowAllowed);

        Transform targetTransform = CameraManager.Instance.SingularityPlacementRefTransform;

        while (Vector3.Distance(m_singularityObject.transform.position, targetTransform.position) > 1f)
        {
            while (GameManager.Instance.GameTimeScale == 0)
                yield return null;

            m_singularityObject.transform.position = Vector3.MoveTowards(
                m_singularityObject.transform.position,
                targetTransform.position,
                m_gameplayData.ComingBackSpeed * Time.deltaTime);

            yield return null;
        }


        m_singularityObject.transform.position = targetTransform.position;

        onComplete?.Invoke();

        m_singularityBehavior.SetIgnoreCollision(false);

        m_isSwitching = false;
    }

    /// <summary>
    /// Waits for a specified duration in seconds, while checking for game pause state. (To avoid WaitForSeconds which uses Time.timeScale)
    /// </summary>
    /// <param name="a_duration"></param>
    /// <returns></returns>
    private IEnumerator WaitForCustomSeconds(float a_duration)
    {
        float timer = 0f;

        while (timer < a_duration)
        {
            while (GameManager.Instance.GameTimeScale == 0)
                yield return null;

            timer += Time.deltaTime;
            yield return null;
        }
    }



    #endregion

    #region Throw
    public void ThrowSingularity()
    {
        if (!m_singularityBehavior.IsAllowedToBeThrown) return;

        if (m_singularityBehavior.IsOverlapping()) return;

        m_singularityBehavior.OnThrow();
        m_characterBehavior.ImobilizeCharacter(true);
        ShowSingularityPreview();
        GameManager.Instance.ChangeMainPlayerState(PlayerState.SINGULARITY, true);
    }

    private void OnThrowPerformed()
    {
        CameraManager.Instance.SwitchCameraToSingularity();
    }
    #endregion

    #region Singularity Jump

    private void OnSingularityJump(Vector3 a_linearVelocityToApply)
    {
        SwitchCharactersPositions();
        m_characterBehavior.OnSingularityJump(a_linearVelocityToApply);
    }

    #endregion

    #region Singularity Dash

    public void SingularityDash(Vector3 a_linearVelocityToApply, Vector3 a_direction)
    {
        SwitchCharactersPositions();
        m_characterBehavior.OnSingularityDash(a_linearVelocityToApply, a_direction);
    }

    #endregion

    #region Manage Singularity Preview

    private void ShowSingularityPreview()
    {
        SingularityPreviewController.Inflate(m_characterObject.transform);
        Invoke("DisableCharacterObject", 0.05f);
    }

    private void DisableCharacterObject() => m_characterObject.SetActive(false);

    private void HideSingularityPreview()
    {
        SingularityPreviewController.Deflate();
        m_characterObject.SetActive(true);
    }

    #endregion
}
