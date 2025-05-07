using BHR;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BHR
{
    public class ModuleManager : ManagerSingleton<ModuleManager>
    {
        [SerializeField, Required, FoldoutGroup("Refs")] private GameObject _transition;
        [Required, FoldoutGroup("Settings")] public float TransitionDuration;
        private bool _haveToLaunchStartAnim = false; 
        public bool SceneTransitionHasFinished;

        [Serializable]
        public enum ModuleType { MAIN_TITLE, LEVEL_SELECTION, CREDITS, PLAYER_SELECTION, MAP_REBINDING, SETTINGS, END_LEVEL, HUD, PAUSE, TEST}
        [SerializeField]
        public SerializedDictionary<GameObject, ModuleType> ModulesRef;
        public GameObject GetModule(ModuleType type) => ModulesRef.First(m => m.Value == type).Key;
        [ReadOnly] public GameObject CurrentModule = null;
        [SerializeField, ReadOnly] private GameObject _moduleToLoadOnSceneLoaded;
        public UnityEvent<GameObject, bool> OnModuleEnabled;
        [SerializeField, ReadOnly] private Stack<GameObject> _historic = new Stack<GameObject>();
        private Selectable _savedBackSelectable;

        [SerializeField, ReadOnly]
        private bool _canBack = true;
        public bool CanBack { get => _canBack; set => _canBack = value;  }

        [FoldoutGroup("UI Settings")] public Color HideMedalColor;
        [FoldoutGroup("UI Settings")] public Color HideMedalTextColor;

        public override void Awake()
        {
            foreach (Transform go in gameObject.transform.GetChild(0).transform)
                if(go.name.ToLower().Contains("module"))
                    go.gameObject.SetActive(false);
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
            ScenesManager.Instance.OnSceneSuccessfulyLoaded.AddListener(LoadModuleOnSceneLoaded);
            GameManager.Instance.OnLaunchLevel.AddListener((startAnimation) => 
            {
                _haveToLaunchStartAnim = startAnimation;
                CheckStartAnimationLaunchConditions();
            });
        }

        public void CheckStartAnimationLaunchConditions()
        {
            if (_haveToLaunchStartAnim && SceneTransitionHasFinished)
            {
                _haveToLaunchStartAnim = SceneTransitionHasFinished = false;
                LaunchStartAnimation();
            }
        }

        private void HandleUIInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && ctx.action.name == InputActions.Cancel)
            {
                OnBack();
            }
        }

        public void SetModuleToLoad(GameObject moduleGO) => _moduleToLoadOnSceneLoaded = moduleGO;
        public void OnModuleEnable(GameObject moduleGO) => ProcessModuleState(moduleGO);

        private void LoadModuleOnSceneLoaded(string sceneName)
        {
            if (_moduleToLoadOnSceneLoaded == null) return;
            ProcessModuleState(_moduleToLoadOnSceneLoaded);
            _moduleToLoadOnSceneLoaded = null;
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

        public void ReloadScene() => ScenesManager.Instance?.ReloadScene(); 

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

        public void OnBack()
        {
            if (CurrentModule.TryGetComponent<AModuleUI>(out AModuleUI moduleScript))
                moduleScript.Back();
            else
                Back();
        }

        public void Back()
        {
            if( _historic.Count > 0 && CanBack)
            {
                ProcessModuleState(_historic.Pop(), false, true);
            }
        }
        public void LaunchTransitionAnimation(bool start) => _transition.GetComponent<TransitionUI>().LaunchTransitionAnimation(start);
        private void LaunchStartAnimation() => _transition.GetComponent<TransitionUI>().LaunchStartAnimation();
    }

}
