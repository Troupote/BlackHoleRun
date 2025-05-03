using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BHR
{
    public class GameManager : ManagerSingleton<GameManager>
    {
        public SettingsSO GameSettings;

        [SerializeField]
        private PlayerState _activePlayerState;
        public PlayerState ActivePlayerState => _activePlayerState;
        private int _activePlayerIndex;
        public int ActivePlayerIndex => _activePlayerIndex;
        private bool _mainPlayerIsPlayerOne = true;
        [SerializeField, ReadOnly]
        private LevelDataSO _selectedLevel = null;
        [SerializeField, ReadOnly]
        private LevelDataSO _currentLevel = null;
        public LevelDataSO SelectedLevel
        {
            get => _selectedLevel;
            private set => _selectedLevel = value;
        }

        [SerializeField, ReadOnly]
        private bool _soloMode = true;
        public bool SoloMode { get => _soloMode; set => _soloMode = value; }

        public LevelDataSO CurrentLevel => _currentLevel;
        public UnityEvent OnLaunchLevel, OnStartLevel;
        public UnityEvent<float, bool> OnEndLevel;

        #region During Game Level
        private float _timer;
        public float Timer
        {
            get => _timer;
            private set
            {
                _timer = value;
                OnTimerChanged.Invoke(_timer);
            }
        }
        public UnityEvent<float> OnTimerChanged;
        [SerializeField, ReadOnly]
        private bool _isPlaying = false;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                _isPaused = !_isPlaying;
                _gameTimeScale = _isPlaying ? _savedGameTimeScale : 0f;
            }
        }
        [SerializeField, ReadOnly]
        private bool _isPaused = false;
        public bool IsPaused
        {
            get => _isPaused;
            private set
            {
                _isPaused = value;
                IsPlaying = !_isPaused;
            }
        }
        private PlayerState _savedPausedState = PlayerState.NONE;
        private float _savedGameTimeScale = 1f;
        [SerializeField, ReadOnly]
        private float _gameTimeScale;
        public float GameTimeScale => _gameTimeScale;
        #endregion

        public UnityEvent<int, PlayerState> OnPlayerStateChanged;

        private void Start()
        {
            Init();

            // Bind to input events
            PlayersInputManager.Instance.OnPause.AddListener(TogglePause);
            PlayersInputManager.Instance.OnRestart.AddListener(RestartLevel);
        }

        private void Update()
        {
            Chrono();
        }

        private void Init()
        {
            _activePlayerState = PlayerState.UI;
        }

        #region Level gestion
        public void SaveSelectedLevel(LevelDataSO data) => SelectedLevel = data;

        public void LaunchLevel()
        {
            PlayersInputManager.Instance.CanConnect = false;
            if(SelectedLevel!=null)
            {
                if(SelectedLevel != CurrentLevel)
                    ScenesManager.Instance.ChangeScene(SelectedLevel);
                else
                    ScenesManager.Instance.ReloadScene();
            }
            else
            {
                Debug.LogError("No selected level !");
                return;
            }
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.HUD));
            ModuleManager.Instance.ClearNavigationHistoric();

            _currentLevel = SelectedLevel;
            _mainPlayerIsPlayerOne = !PlayersInputManager.Instance.IsSwitched;

            Timer = 0f;
            IsPlaying = false;
            OnLaunchLevel.Invoke();
        }

        public void StartLevel()
        {
            IsPlaying = true; OnStartLevel.Invoke();
            ChangeMainPlayerState(PlayerState.HUMANOID, PlayersInputManager.Instance.IsSwitched);
        }

        public void TogglePause()
        {
            if (IsPaused && ModuleManager.Instance.CurrentModule == ModuleManager.Instance.GetModule(ModuleManager.ModuleType.PAUSE))
                Resume(); 
            else if(!IsPaused && ModuleManager.Instance.CurrentModule == ModuleManager.Instance.GetModule(ModuleManager.ModuleType.HUD))
                Pause(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.PAUSE));
        }

        public void Pause(GameObject moduleToLoad)
        {
            Cursor.lockState = CursorLockMode.None;
            if(!IsPaused)
            {
                _savedGameTimeScale = GameTimeScale;
                _savedPausedState = ActivePlayerState;
                IsPaused = true;
                ChangeMainPlayerState(PlayerState.UI, false);
            }
            ModuleManager.Instance.OnModuleEnable(moduleToLoad);
        }

        public void Resume()
        {
            Cursor.lockState = CursorLockMode.Locked;
            IsPaused = false;
            ChangeMainPlayerState(_savedPausedState, false);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.HUD));
        }

        public void EndLevel()
        {
            CleanInGame();
            IsPlaying = false;
            bool newBestTime = _currentLevel.SaveTime(Timer);
            ChangeMainPlayerState(PlayerState.UI, false);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.END_LEVEL));
            ModuleManager.Instance.ClearNavigationHistoric();
            OnEndLevel.Invoke(Timer, newBestTime);
        }

        public void QuitLevel()
        {
            CleanInGame();
            ChangeMainPlayerState(PlayerState.UI, false);
            ScenesManager.Instance.ChangeScene(ScenesManager.Instance.MenuScene);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.LEVEL_SELECTION));
            ModuleManager.Instance.ClearNavigationHistoric();
        }

        public void RestartLevel()
        {
            Resume();
            ChangeMainPlayerState(PlayerState.UI, false);
            LaunchLevel();
        }
        #endregion
        public void CleanInGame()
        {
            // Clean all we need to clean
            CharactersManager.Instance.DestroyInstance();
            CameraManager.Instance.DestroyInstance();
        }

        private void Chrono()
        {
            if (IsPlaying)
                Timer += Time.deltaTime * GameTimeScale;
        }

        private const float _outerWildsEasterEggBonus = -0.22f;
        public void ILoveOuterWidls()
        {
            Timer += _outerWildsEasterEggBonus;
        }

        public void ChangeMainPlayerState(PlayerState state, bool switchActivePlayer)
        {
            if(switchActivePlayer) _mainPlayerIsPlayerOne = !_mainPlayerIsPlayerOne;

            _activePlayerState = state;

            // SoloMode version
            if(_soloMode)
            {
                _activePlayerIndex = Array.IndexOf(PlayersInputManager.Instance.PlayersReadyState, PlayerReadyState.READY);
                PlayersInputManager.Instance.PlayersInputControllerRef[_activePlayerIndex].GetComponent<PlayerInputController>().PlayerState = _activePlayerState;
            }
            else
            {
                _activePlayerIndex = _mainPlayerIsPlayerOne ? 0 : 1;
                PlayersInputManager.Instance.PlayersInputControllerRef[_activePlayerIndex].GetComponent<PlayerInputController>().PlayerState = _activePlayerState;

                PlayerState secondPlayerState = _activePlayerState == PlayerState.UI ? PlayerState.UI : PlayerState.INACTIVE;
                PlayersInputManager.Instance.PlayersInputControllerRef[1-_activePlayerIndex].GetComponent<PlayerInputController>().PlayerState = secondPlayerState;
            }
        }
    }
}
