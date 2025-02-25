using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CharactersManager : MonoBehaviour
{
    /* Is used to spawn characters */

    [SerializeField]
    private GameObject _characterPrefab;

    [SerializeField]
    private GameObject _singularityPrefab;

    private GameObject _singularityObject;
    private GameObject _characterObject;

    private SingularityBehavior a_singularityBehavior;
    private CharacterBehavior a_characterBehavior;

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
        SpawnCharacterAtPosition(Vector3.zero + new Vector3(0,5,0));
    }

    private void InstanciatePrefabsOnScene()
    {
        _characterObject = Instantiate(_characterPrefab);
        _singularityObject = Instantiate(_singularityPrefab);

        a_singularityBehavior = _singularityObject.GetComponent<SingularityBehavior>();
        a_characterBehavior = _characterObject.GetComponent<CharacterBehavior>();

        a_characterBehavior.SetDependencies(a_singularityBehavior);
        CameraSwitcher.Instance.SetDependencies(_characterObject, _singularityObject);
    }

    private bool AreObjectsInstancied()
    {
        if (_characterObject != null && _singularityObject != null && a_singularityBehavior != null && a_characterBehavior != null)
        {
            return true;
        }

        return false;
    }

    public void SpawnCharacterAtPosition(Vector3 a_position)
    {
        if (_characterObject == null && _singularityObject == null)
        {
            InstanciatePrefabsOnScene();
        }

        _characterObject.transform.position = a_position;

    }

    private void SwitchCharacterToSingularity()
    {

    }

    private void SwitchSingularityToCharacter()
    {

    }

    public void ChangePlayersTurn()
    {
        SwitchCharacterToSingularity();
        SwitchSingularityToCharacter();

        _characterObject.transform.position = _singularityObject.transform.position + new Vector3(0, 4, 0);
        CameraSwitcher.Instance.SwitchCameraToCharacter();
        a_singularityBehavior.ShouldAllowThrowAgain(true);
    }

    private float timeElasped = 0;

    private void Update()
    {
        if (!AreObjectsInstancied()) return;

        if (!a_singularityBehavior.IsThrown)
        {
            a_singularityBehavior.FollowPlayer(_characterObject.transform.position);
            timeElasped = 0;
        }
        else
        {
            timeElasped += Time.deltaTime;

            if (timeElasped > 4f)
            {
                ChangePlayersTurn();
                timeElasped = 0;
            }
        }
    }
}
