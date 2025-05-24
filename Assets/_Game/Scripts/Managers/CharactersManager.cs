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

    [SerializeField]
    private LayerMask groundLayer;

    private GameObject m_singularityObject;
    private GameObject m_characterObject;
    public GameObject CharacterObject => m_characterObject;

    private SingularityBehavior m_singularityBehavior;
    private CharacterBehavior m_characterBehavior;

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
    }

    private void UnlistenToEvents()
    {
        m_singularityBehavior.OnThrowPerformed -= OnThrowPerformed;
        m_characterBehavior.OnThrowInput -= ThrowSingularity;
        m_singularityBehavior.OnUnmorph -= SwitchCharactersPositions;
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

    #region Life Cycle

    private void FixedUpdate()
    {
        if (!AreObjectsInstancied()) return;

    }

    #endregion

    #region Switch Characters

    float GetCharacterHeight()
    {
        CapsuleCollider collider = m_characterObject.GetComponent<CapsuleCollider>();
        return collider ? collider.height : 2.0f;
    }

    public void SwitchCharactersPositions()
    {
        Vector3 singularityPosition = m_singularityObject.transform.position;
        RaycastHit hit;
        // To make sure the character does not get stuck underground
        if (Physics.Raycast(singularityPosition + Vector3.up * 0.5f, Vector3.down, out hit, 2.0f, groundLayer))
        {
            singularityPosition.y = hit.point.y + GetCharacterHeight();
        }

        var oldCharacterPosition = m_characterObject.transform.position;
        m_characterObject.transform.position = singularityPosition;
        m_singularityObject.transform.position = oldCharacterPosition;

        CameraManager.Instance.SwitchCameraToCharacter(m_characterObject.transform.position);

        m_characterBehavior.ImobilizeCharacter(false);

        GameManager.Instance.ChangeMainPlayerState(PlayerState.HUMANOID, false);

        StartCoroutine(MoveSlowlySingularityToNewCharacter(() =>
        {
            m_singularityBehavior.SingularityCharacterFollowComponent.PickupSingularity(true);
        }));
    }

    internal bool SingularityMovingToCharacter { get; private set; } = false;
    private IEnumerator MoveSlowlySingularityToNewCharacter(Action onComplete = null)
    {
        SingularityMovingToCharacter = true;

        float elapsed = 0f;

        Vector3 start = m_singularityObject.transform.position;

        while (elapsed < 2f)
        {
            float t = elapsed / 2f;
            float curveT = m_gameplayData.JoinBackToCharacterSpeed.Evaluate(t); // To redo maybe
            Vector3 currentTarget = CameraManager.Instance.SingularityPlacementRefTransform.position;

            m_singularityObject.transform.position = Vector3.Lerp(start, currentTarget, curveT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        m_singularityObject.transform.position = CameraManager.Instance.SingularityPlacementRefTransform.position;

        onComplete?.Invoke();

        SingularityMovingToCharacter = false;
    }

    #endregion

    #region Throw
    public void ThrowSingularity()
    {
        if (!m_singularityBehavior.IsAllowedToBeThrown) return;

        m_singularityBehavior.OnThrow();
        m_characterBehavior.ImobilizeCharacter(true);
        GameManager.Instance.ChangeMainPlayerState(PlayerState.SINGULARITY, true);
    }

    private void OnThrowPerformed()
    {
        CameraManager.Instance.SwitchCameraToSingularity();
    }
    #endregion
}
