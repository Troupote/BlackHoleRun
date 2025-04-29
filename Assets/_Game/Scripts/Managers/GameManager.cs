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
        private LevelDataSO _selectedLevel = null;
        private LevelDataSO _currentLevel = null;
        public LevelDataSO SelectedLevel
        {
            get => _selectedLevel;
            set => _selectedLevel = value;
        }

        [SerializeField, ReadOnly]
        private bool _soloMode = true;
        public bool SoloMode { get => _soloMode; set => _soloMode = value; }

        public LevelDataSO CurrentLevel => _currentLevel;
        public UnityEvent OnLaunchLevel, OnStartLevel;
        public UnityEvent<float> OnEndLevel;

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
        private float _gameTimeScale;
        public float GameTimeScale => _gameTimeScale;
        #endregion

        public UnityEvent<int, PlayerState> OnPlayerStateChanged;

        private void Start()
        {
            Init();

            // Bind to input events
            PlayersInputManager.Instance.OnPause.AddListener(TogglePause);
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
            ScenesManager.Instance.ChangeScene(SelectedLevel);
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
            if (!IsPaused) Pause(); else Resume();
        }

        private void Pause()
        {
            _savedGameTimeScale = GameTimeScale;
            _savedPausedState = ActivePlayerState;
            IsPaused = true;
            //ChangeMainPlayerState(PlayerState.UI, false);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.PAUSE));
        }

        public void Resume()
        {
            IsPaused = false;
            //ChangeMainPlayerState(_savedPausedState, false);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.HUD));
        }

        public void EndLevel()
        {
            CleanInGame();
            IsPlaying = false;
            _currentLevel.SaveTime(Timer);
            ChangeMainPlayerState(PlayerState.UI, false);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.END_LEVEL));
            ModuleManager.Instance.ClearNavigationHistoric();
            OnEndLevel.Invoke(Timer);
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

        private void Update()
        {
            if (IsPlaying)
                Timer += Time.deltaTime * GameTimeScale;
        }

        public void ChangeMainPlayerState(PlayerState state, bool switchActivePlayer)
        {
            if(switchActivePlayer) _mainPlayerIsPlayerOne = !_mainPlayerIsPlayerOne;

            _activePlayerState = state;

            // SoloMode version
            if(_soloMode)
            {
                _activePlayerIndex = Array.IndexOf(PlayersInputManager.Instance.PlayersReadyState, PlayerReadyState.READY);
                PlayersInputManager.Instance.PlayersInputRef[_activePlayerIndex].GetComponent<PlayerInputController>().PlayerState = _activePlayerState;
            }
            else
            {
                _activePlayerIndex = _mainPlayerIsPlayerOne ? 0 : 1;
                PlayersInputManager.Instance.PlayersInputRef[_activePlayerIndex].GetComponent<PlayerInputController>().PlayerState = _activePlayerState;

                PlayerState secondPlayerState = _activePlayerState == PlayerState.UI ? PlayerState.UI : PlayerState.INACTIVE;
                PlayersInputManager.Instance.PlayersInputRef[1-_activePlayerIndex].GetComponent<PlayerInputController>().PlayerState = secondPlayerState;
            }
        }
    }
}
