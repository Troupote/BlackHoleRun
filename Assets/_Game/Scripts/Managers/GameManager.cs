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
        public bool SoloMode { get => _soloMode; set { _soloMode = value; if (_soloMode) _hasPlayedInSolo = true; } }
        private bool _hasPlayedInSolo = false;
        public bool HasPlayedInSolo => _hasPlayedInSolo;

        public LevelDataSO CurrentLevel => _currentLevel;
        public UnityEvent<bool> OnLaunchLevel;
        public UnityEvent OnStartLevel;
        /// <summary>
        /// Float End timer, bool HasHitNewBestTime, bool HasPlayedSolo
        /// </summary>
        public UnityEvent<float, bool, bool> OnEndLevel;

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
                if (_isPlaying)
                    IsPaused = false;
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
                if(_isPaused)
                    IsPlaying = false;
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
            PlayersInputManager.Instance.OnRestart.AddListener(() => RestartLevel(false));
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

        public void LaunchLevel(bool firstStart = true)
        {
            SoloMode = PlayersInputManager.Instance.SoloModeEnabled;
            PlayersInputManager.Instance.CanConnect = false;
            ModuleManager.Instance.SetModuleToLoad(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.HUD));

            if(SelectedLevel!=null)
            {
                if(SelectedLevel != CurrentLevel)
                    ScenesManager.Instance.ChangeScene(SelectedLevel, false);
                else
                    ScenesManager.Instance.ReloadScene(firstStart);
            }
            else
            {
                Debug.LogError("No selected level !");
                return;
            }


            ModuleManager.Instance.ClearNavigationHistoric();

            _currentLevel = SelectedLevel;
            _mainPlayerIsPlayerOne = !PlayersInputManager.Instance.IsSwitched;

            Timer = 0f;
            IsPlaying = false;
            OnLaunchLevel.Invoke(firstStart);
            if(!firstStart) StartLevel();
        }

        public void StartLevel()
        {
            Cursor.lockState = CursorLockMode.Locked;
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
            IsPlaying = true;
            Cursor.lockState = CursorLockMode.Locked;
            ChangeMainPlayerState(_savedPausedState, false);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.HUD));
        }

        public void EndLevel()
        {
            CleanInGame(false);
            IsPlaying = false;
            bool newBestTime = false;
            if (_hasPlayedInSolo)
                newBestTime = CurrentLevel.SaveTime(Timer);
            ChangeMainPlayerState(PlayerState.UI, false);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.END_LEVEL));
            ModuleManager.Instance.ClearNavigationHistoric();
            OnEndLevel.Invoke(Timer, newBestTime, _hasPlayedInSolo);
        }

        public void QuitLevel()
        {
            CleanInGame(false);
            IsPaused = false;
            ChangeMainPlayerState(PlayerState.UI, false);
            ModuleManager.Instance.SetModuleToLoad(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.LEVEL_SELECTION));
            ScenesManager.Instance.ChangeScene(ScenesManager.Instance.MenuScene);
            ModuleManager.Instance.ClearNavigationHistoric();
            CleanInGame(true);
        }

        public void RestartLevel(bool withStartAnimation = false)
        {
            Resume();
            ChangeMainPlayerState(PlayerState.UI, false);
            LaunchLevel(withStartAnimation);
        }
        #endregion
        public void CleanInGame(bool late)
        {
            if(!late)
            {
                // Clean all we need to clean immediatly
                CharactersManager.Instance.DestroyInstance();
                CameraManager.Instance.DestroyInstance();
            }
            else // Cleann all we need to clean after end level gestion
            {
                _hasPlayedInSolo = false;
                _currentLevel = null;
            }
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
