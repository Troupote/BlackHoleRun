using BHR;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class ModuleManager : MonoBehaviour
{
    public static ModuleManager Instance;
    [Header("Modules data"), SerializeField]
    private GameObject MainTitle;
    [SerializeField]
    private GameObject Settings;
    [SerializeField]
    private GameObject MapRebinding;
    [SerializeField]
    private GameObject Credits;
    [SerializeField]
    private GameObject LevelSelection;
    [SerializeField]
    private GameObject PlayerSelection;
    [SerializeField]
    private GameObject Pause;

    [SerializeField, ReadOnly] private GameObject _currentModule = null;
    [SerializeField, ReadOnly] private Stack<GameObject> _historic = new Stack<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void Init()
    {
        _currentModule = null;
        _historic = new Stack<GameObject>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            Back();
    }

    #region Button functions
    public void MainTitleModule(bool cannotReturn)
    {
        ProcessModuleState(MainTitle, cannotReturn);
    }

    public void SettingsModule(bool cannotReturn)
    {
        ProcessModuleState(Settings, cannotReturn);
    }

    public void MapRebindingModule(bool cannotReturn)
    {
        ProcessModuleState(MapRebinding, cannotReturn);
    }

    public void CreditsModule(bool cannotReturn)
    {
        ProcessModuleState(Credits, cannotReturn);
    }

    public void LevelSelectionModule(bool cannotReturn)
    {
        ProcessModuleState(LevelSelection, cannotReturn);
    }

    public void PlayerSelectionModule(bool cannotReturn)
    {
        ProcessModuleState(PlayerSelection, cannotReturn);
    }

    public void PauseModule(bool cannotReturn)
    {
        ProcessModuleState(Pause, cannotReturn);
    }

    public void ChangeScene(string scene)
    {
        ScenesManager.Instance.ChangeScene(scene);
    }
    #endregion

    private void ProcessModuleState(GameObject module, bool cannotReturn = false, bool back = false)
    {
        module.SetActive(true);

        if(cannotReturn || back)
        {
            if(cannotReturn)
                _historic.Clear();
        }
        else
            _historic.Push(_currentModule);

        _currentModule?.SetActive(false);
        _currentModule = module;
    }

    public void Back()
    {
        if( _historic.Count > 0 )
        {
            ProcessModuleState(_historic.Pop(), false, true);
        }
        else if(_currentModule == LevelSelection) // Specific case after returning to menu from Pause module, it's a SMALL SMALL SMALL detail but ENJMIN said "c'est pas ergo" ;)
        {
            ProcessModuleState(MainTitle, false, true);
        }
    }
}
