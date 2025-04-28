using BHR;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BHR
{
    public class ModuleManager : ManagerSingleton<ModuleManager>
    {

        [Serializable]
        public enum ModuleType { MAIN_TITLE, LEVEL_SELECTION, CREDITS, PLAYER_SELECTION, MAP_REBINDING, SETTINGS, END_LEVEL, HUD, PAUSE, TEST}
        [SerializeField]
        public SerializedDictionary<GameObject, ModuleType> ModulesRef;
        public GameObject GetModule(ModuleType type) => ModulesRef.First(m => m.Value == type).Key;
        [ReadOnly] public GameObject CurrentModule = null;
        public UnityEvent<GameObject, bool> OnModuleEnabled;
        [SerializeField, ReadOnly] private Stack<GameObject> _historic = new Stack<GameObject>();
        private Selectable _savedBackSelectable;

        [SerializeField, ReadOnly]
        private bool _canBack = true;
        public bool CanBack { get => _canBack; set => _canBack = value;  }
    #if UNITY_EDITOR
        public bool TestMode;
    #endif

        [FoldoutGroup("UI Settings")] public Color HideMedalColor;
        [FoldoutGroup("UI Settings")] public Color HideMedalTextColor;

        public override void Awake()
        {
            foreach (Transform go in gameObject.transform)
                go.gameObject.SetActive(!go.name.ToLower().Contains("module"));
            base.Awake();
        }

        private void Start()
        {
            Init();
        }
        public void Init()
        {
            _historic = new Stack<GameObject>();

            PlayersInputManager.Instance.OnUIInput.AddListener(HandleUIInput);
        }

        private void HandleUIInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && ctx.action.name == InputActions.Cancel)
            {
                Back();
            }
        }

        public void OnModuleEnable(GameObject moduleGO)
        {
            ProcessModuleState(moduleGO);
        }

        public void SaveBackSelectable(Selectable selectable)
        {
            _savedBackSelectable = selectable;
        }

        public void SelectBackSelectable()
        {
            _savedBackSelectable?.Select();
            _savedBackSelectable = null;
        }

        public void ClearNavigationHistoric()
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
            if (module == null || module == CurrentModule)
                return;

            module.SetActive(true);

            if(cannotReturn || back)
            {
                if(cannotReturn)
                    ClearNavigationHistoric();
            }
            else
                _historic.Push(CurrentModule);
            if(CurrentModule != null)
                CurrentModule?.SetActive(false);
            CurrentModule = module;
            //Debug.Log($"CurrentModule : {CurrentModule}");

            OnModuleEnabled.Invoke(module, _savedBackSelectable != null && back);
            if (back)
                SelectBackSelectable();
        }

        public void Back()
        {
            if( _historic.Count > 0 && CanBack)
            {
                ProcessModuleState(_historic.Pop(), false, true);
            }
        }
    }

}
