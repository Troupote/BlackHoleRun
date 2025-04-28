using System.Collections;
using UnityEngine;

public class CharactersManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _characterPrefab;

    [SerializeField]
    private GameObject _singularityPrefab;

    [SerializeField]
    private GameplayData m_gameplayData;

    internal GameplayData GameplayData => m_gameplayData;

    [SerializeField]
    private LayerMask groundLayer;

    private GameObject _singularityObject;
    private GameObject _characterObject;

    private SingularityBehavior a_singularityBehavior;
    private CharacterBehavior a_characterBehavior;

    internal bool SingularityThrown = false;

    public static CharactersManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SpawnCharacterAtPosition(Vector3.zero + new Vector3(0, 5, 0));
    }

    private void InstanciatePrefabsOnScene()
    {
        _characterObject = Instantiate(_characterPrefab);
        _singularityObject = Instantiate(_singularityPrefab);

        a_singularityBehavior = _singularityObject.GetComponent<SingularityBehavior>();
        a_characterBehavior = _characterObject.GetComponent<CharacterBehavior>();

        CameraManager.Instance.SetDependencies(_characterObject, _singularityObject);
    }

    private bool AreObjectsInstancied()
    {
        return _characterObject != null && _singularityObject != null && a_singularityBehavior != null && a_characterBehavior != null;
    }

    public void SpawnCharacterAtPosition(Vector3 a_position)
    {
        if (!AreObjectsInstancied())
        {
            InstanciatePrefabsOnScene();
        }

        _characterObject.transform.position = a_position;
    }

    float GetCharacterHeight()
    {
        CapsuleCollider collider = _characterObject.GetComponent<CapsuleCollider>();
        return collider ? collider.height : 2.0f;
    }

    public void SwitchCharacterAndSingularity()
    {
        Vector3 singularityPosition = _singularityObject.transform.position;
        RaycastHit hit;
        if (Physics.Raycast(singularityPosition + Vector3.up * 0.5f, Vector3.down, out hit, 2.0f, groundLayer))
        {
            singularityPosition.y = hit.point.y + GetCharacterHeight();
        }

        a_characterBehavior.ResetVelocity();

        Vector3 oldCharacterPosition = _characterObject.transform.position;
        _characterObject.transform.position = singularityPosition;
        _singularityObject.transform.position = oldCharacterPosition;

        a_characterBehavior.ImobilizeCharacter(false);
    }

    public void ChangePlayersTurn(bool a_isEarly = false)
    {
        StartCoroutine(WaitForBlendingAndSwitch(a_isEarly));
    }

    private IEnumerator WaitForBlendingAndSwitch(bool a_isEarly)
    {
        if (!a_isEarly)
        {
            yield return new WaitUntil(() => !CameraManager.Instance.IsBlending);
        }

        SwitchCharacterAndSingularity();
        CameraManager.Instance.SwitchCameraToCharacter(_characterObject.transform.position);
        a_singularityBehavior.ResetThrowState(true);
    }

    public void IsSingularityThrown(bool a_isIt)
    {
        SingularityThrown = a_isIt;
    }

    public void TryThrowSingularity()
    {
        if (a_singularityBehavior.IsThrown && !a_singularityBehavior.AlreadyCollided)
        {
            ChangePlayersTurn(true);
            return;
        }

        if (!a_characterBehavior.HasTouchedGround) return;

        a_singularityBehavior.Throw();
        a_characterBehavior.ImobilizeCharacter(true);
        IsSingularityThrown(true);
    }

    private float timeElasped = 0;

    private void Update()
    {
        if (!AreObjectsInstancied()) return;
        if (a_singularityBehavior == null || _characterObject == null) return;

        if (!a_singularityBehavior.IsThrown)
        {
            if (timeElasped > 0) timeElasped = 0;
        }
        else
        {
            timeElasped += Time.deltaTime;

            if (timeElasped >= m_gameplayData.SecondsBeforeSpawningCharacterBackIfNoCollision)
            {
                ChangePlayersTurn();
                timeElasped = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!AreObjectsInstancied()) return;

        if (!a_singularityBehavior.IsThrown)
        {
            a_singularityBehavior.FollowPlayer();
        }
    }
}
