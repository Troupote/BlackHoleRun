using BHR;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class ModuleManager : MonoBehaviour
{
    public static ModuleManager Instance;
    [SerializeField, ReadOnly] private GameObject _currentModule = null;
    [SerializeField, ReadOnly] private Stack<GameObject> _historic = new Stack<GameObject>();

    private bool _canBack = true;
    public bool CanBack { get => _canBack; set => _canBack = value;  }

    public GameObject MainMenuDefaultModule;
    public GameObject LevelDefaultModule;
#if UNITY_EDITOR
    public GameObject TestDefaultModule;
#endif

    [FoldoutGroup("UI Settings")] public Color HideMedalColor;
    [FoldoutGroup("UI Settings")] public Color HideMedalTextColor;

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

    public void OnModuleEnable(GameObject moduleGO)
    {
        ProcessModuleState(moduleGO);
    }

    public void ClearHistoric()
    {
        _historic.Clear();
    }

    public void ChangeScene(SceneDataSO sceneData)
    {
        ScenesManager.Instance.ChangeScene(sceneData);
    }

    public void ReloadScene()
    {
        ScenesManager.Instance.ChangeScene(ScenesManager.Instance.CurrentSceneData);
    }

    public void QuitGame()
    {
        ScenesManager.Instance.QuitGame();
    }

    public void ProcessModuleState(GameObject module, bool cannotReturn = false, bool back = false)
    {
        module.SetActive(true);

        if(cannotReturn || back)
        {
            if(cannotReturn)
                ClearHistoric();
        }
        else
            _historic.Push(_currentModule);

        _currentModule?.SetActive(false);
        _currentModule = module;
    }

    public void Back()
    {
        if( _historic.Count > 0 && CanBack)
        {
            ProcessModuleState(_historic.Pop(), false, true);
        }
    }
}
