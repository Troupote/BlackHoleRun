using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BHR
{
    public class GameManager : ManagerSingleton<GameManager>
    {
        [Required]
        public GameSettingsSO GameSettings;

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
            set => _selectedLevel = value;
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
        private bool _isRespawning = false;
        private bool IsRespawning => _isRespawning;

        private PlayerState _savedPausedState = PlayerState.NONE;
        private float _savedGameTimeScale = 1f;
        [SerializeField, ReadOnly]
        private float _gameTimeScale;
        public float GameTimeScale => _gameTimeScale;
        #endregion

        public UnityEvent<PlayerState, bool> OnMainPlayerStateChanged;
        public UnityEvent OnPaused, OnResumed, OnRespawned;

        private void Start()
        {
            Init();

            // Bind to input events
            PlayersInputManager.Instance.OnPause.AddListener(TogglePause);
            PlayersInputManager.Instance.OnRestartLevel.AddListener(() => RestartLevel(false));
            PlayersInputManager.Instance.OnRespawn.AddListener(Respawning);
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
            ModuleManager.Instance.SetModuleToLoad(ModuleManager.Instance.GetModule(ModuleType.HUD));

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
            CheckpointsManager.Instance.ReplacePlayer();
            IsPlaying = true; OnStartLevel.Invoke();
            ChangeMainPlayerState(PlayerState.HUMANOID, PlayersInputManager.Instance.IsSwitched);
        }

        public void TogglePause()
        {
            if (IsPaused && ModuleManager.Instance.CurrentModule == ModuleManager.Instance.GetModule(ModuleType.PAUSE))
                Resume(); 
            else if(!IsPaused && ModuleManager.Instance.CurrentModule == ModuleManager.Instance.GetModule(ModuleType.HUD))
                Pause(ModuleManager.Instance.GetModule(ModuleType.PAUSE));
        }

        public void Pause(GameObject moduleToLoad)
        {
            OnPaused?.Invoke();
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
            OnResumed?.Invoke();
            IsPlaying = true;
            ChangeMainPlayerState(_savedPausedState, false);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleType.HUD));
            ModuleManager.Instance.ClearNavigationHistoric();
        }

        public void EndLevel()
        {
            OnPaused?.Invoke();
            CleanInGame(false);
            IsPlaying = false;
            bool newBestTime = false;
            if (_hasPlayedInSolo)
                newBestTime = CurrentLevel.SaveTime(Timer);
            ChangeMainPlayerState(PlayerState.UI, false);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleType.END_LEVEL));
            ModuleManager.Instance.ClearNavigationHistoric();
            OnEndLevel.Invoke(Timer, newBestTime, _hasPlayedInSolo);
        }

        public void QuitLevel()
        {
            CleanInGame(false);
            IsPaused = false;
            ChangeMainPlayerState(PlayerState.UI, false);
            ModuleManager.Instance.SetModuleToLoad(ModuleManager.Instance.GetModule(ModuleType.LEVEL_SELECTION));
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

        #region InGame gestion
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

        public void Respawning()
        {
            ApplyRespawn();
            //Coroutine respawnPlayer = StartCoroutine(RespawnPlayerAnimation(() => ApplyRespawn()));
        }

        private void ApplyRespawn()
        {
            OnRespawned?.Invoke();
            //ModuleManager.Instance.LaunchTransitionAnimation(false, GameSettings.RespawningDuration / 2f);
            //_isRespawning = false;
            //Invoke("Play", GameSettings.RespawningDuration / 2f);
        }

        private void Play() => IsPlaying = true;

        IEnumerator RespawnPlayerAnimation(Action onComplete = null)
        {
            float transitionDuration = GameSettings.RespawningDuration / 2f;
            IsPaused = true;
            _isRespawning = true;
            ModuleManager.Instance.LaunchTransitionAnimation(true, transitionDuration);
            yield return new WaitForSeconds(transitionDuration + 0.5f);
            onComplete?.Invoke();
            //CheckpointsManager.Instance.ReplacePlayer();
            //OnRespawn?.Invoke();
            //ModuleManager.Instance.LaunchTransitionAnimation(false, transitionDuration);
            //yield return new WaitForSeconds(transitionDuration);
            //_isRespawning = false;
            //IsPlaying = true;
        }

        public void LoadTutorielData(TutorielData data)
        {
            ModuleManager.Instance.GetModule(ModuleType.TUTORIEL).GetComponent<TutorielModuleUI>().LoadTutorielData(data);
            Pause(ModuleManager.Instance.GetModule(ModuleType.TUTORIEL));
            ModuleManager.Instance.ClearNavigationHistoric();
        }


        public void ChangeMainPlayerState(PlayerState state, bool switchActivePlayer)
        {
            if(switchActivePlayer) _mainPlayerIsPlayerOne = !_mainPlayerIsPlayerOne;

            Cursor.lockState = state == PlayerState.UI ? CursorLockMode.None : CursorLockMode.Locked;

            _activePlayerState = state;

            // SoloMode version
            if(_soloMode)
            {
                _activePlayerIndex = Array.IndexOf(PlayersInputManager.Instance.PlayersReadyState, PlayerReadyState.READY);
                PlayersInputManager.Instance.PlayersInputControllerRef[_activePlayerIndex].GetComponent<PlayerInputController>().PlayerState = _activePlayerState;

                if (PlayersInputManager.Instance.PlayersInputControllerRef.Where(p => p != null).ToArray().Length == 2)
                    PlayersInputManager.Instance.PlayersInputControllerRef[1 - _activePlayerIndex].GetComponent<PlayerInputController>().PlayerState = state == PlayerState.UI ? state : PlayerState.INACTIVE;
            }
            else
            {
                _activePlayerIndex = _mainPlayerIsPlayerOne ? 0 : 1;
                PlayersInputManager.Instance.PlayersInputControllerRef[_activePlayerIndex].GetComponent<PlayerInputController>().PlayerState = _activePlayerState;

                PlayerState secondPlayerState = _activePlayerState == PlayerState.UI ? PlayerState.UI : PlayerState.INACTIVE;
                PlayersInputManager.Instance.PlayersInputControllerRef[1-_activePlayerIndex].GetComponent<PlayerInputController>().PlayerState = secondPlayerState;
            }
            OnMainPlayerStateChanged?.Invoke(state, switchActivePlayer);
        }
        #endregion

        #region Time gestion
        private void Chrono()
        {
            if (IsPlaying || IsRespawning)
                Timer += Time.deltaTime * (IsPlaying ? GameTimeScale : _savedGameTimeScale);
        }

        private const float _outerWildsEasterEggBonus = -0.22f;
        public void ILoveOuterWidls()
        {
            Timer += _outerWildsEasterEggBonus;
        }


        public bool isTimeSlowed = true;
        public bool isSlowMotionSequenceFinished = false;
        public bool isSlowMotionSequenceStarted = false;
        public IEnumerator SlowmotionSequence(float inDuration, float outDuration)
        {
            if (!isSlowMotionSequenceStarted)
            {
                isSlowMotionSequenceStarted = true;
                isTimeSlowed = true;
                isSlowMotionSequenceFinished = false;

                StartCoroutine(ChangeTimeScale(GameTimeScale, CharactersManager.Instance.GameplayData.TargetAimTimeScale, inDuration));

                //Wait until isSlowed becomes false
                yield return new WaitUntil(() => isTimeSlowed == false);

                StartCoroutine(ChangeTimeScale(GameTimeScale, 1f, outDuration));

                isSlowMotionSequenceFinished = true;
            }
        }

        IEnumerator ChangeTimeScale(float start, float end, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                _gameTimeScale = Mathf.Lerp(start, end, elapsed / duration);
                //Time.fixedDeltaTime = Time.timeScale * 0.02f;// ? que faire
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _gameTimeScale = end;
        }

        #endregion
    }
}
