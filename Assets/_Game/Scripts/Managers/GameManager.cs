using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BHR
{
    public class GameManager : ManagerSingleton<GameManager>
    {
        [Required]
        public GameSettingsSO GameSettings;

        [Required]
        public LevelDataSO VSLevelData;

        private CharacterGameplayData _characterGameplayData => CharactersManager.Instance.GameplayData;

        [SerializeField]
        private PlayerState _activePlayerState;
        public PlayerState ActivePlayerState => _activePlayerState;
        private int _activePlayerIndex;
        public int ActivePlayerIndex => _activePlayerIndex;
        public bool _mainPlayerIsPlayerOne = true;
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
        public bool SoloMode { get => _soloMode; set { _soloMode = value; if (_soloMode) { _hasPlayedInSolo = true; } } }
        private bool _hasPlayedInSolo = false;
        public bool HasPlayedInSolo => _hasPlayedInSolo;

        public LevelDataSO CurrentLevel => _currentLevel;
        public UnityEvent<bool> OnLaunchLevel, OnTutorielSet;
        private bool _tutorielEnable;
        public TutorielData SavedTutorielData;
        public bool CanOpenPopup = false;
        public UnityEvent OnStartLevel;
        /// <summary>
        /// Float End timer, bool HasHitNewBestTime, bool HasPlayedSolo
        /// </summary>
        public UnityEvent<float, bool, bool, bool> OnEndLevel;

        #region During Game Level
        private float _timer;
        public float Timer
        {
            get => _timer;
            private set
            {
                _timer = value;
            }
        }
        private float _timerBeforeCollision;
        public float TimerBeforeCollision
        {
            get => _timerBeforeCollision;
            private set
            {
                _timerBeforeCollision = value;
            }
        }
        public UnityEvent<float, bool> OnTimerChanged;
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
                GameTimeScale = _isPlaying ? _savedGameTimeScale : 0f;
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
        public float GameTimeScale
        {
            get => _gameTimeScale;
            private set
            {
                _gameTimeScale = value;
                OnGameTimeScaleChanged?.Invoke(_gameTimeScale);
            }
        }
        public UnityEvent<float> OnGameTimeScaleChanged;

        [SerializeField] private Material _speedLines;
        #endregion

        public UnityEvent<PlayerState, bool> OnMainPlayerStateChanged;
        public UnityEvent OnPaused, OnResumed, OnRespawned;


        public bool IsPracticeMode = false;
        private void Start()
        {
            Init();
            ChangeSpeedLines(SpeedLinesState.NONE);

            // Bind to input events
            PlayersInputManager.Instance.OnPause.AddListener(TogglePause);
            PlayersInputManager.Instance.OnRestartLevel.AddListener(() => RestartLevel(false));
            PlayersInputManager.Instance.OnRespawn.AddListener(Respawning);
            PlayersInputManager.Instance.OnOpenTutorial.AddListener(() => TryLoadTutorielData(SavedTutorielData));
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

        public void SetTutoriel(bool enable) => _tutorielEnable = enable;

        public void LaunchLevel(bool firstStart = true)
        {
            ChangeSpeedLines(SpeedLinesState.NONE);
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
            TimerBeforeCollision = SelectedLevel.Times[MedalsType.EARTH];
            IsPlaying = false;
            OnLaunchLevel.Invoke(firstStart);
            if(!firstStart) StartLevel();
        }

        public void PreAnimationStartLevel()
        {
            CheckpointsManager.Instance.ReplacePlayer();
            OnTutorielSet?.Invoke(_tutorielEnable);
            _tutorielEnable = false;
#if UNITY_EDITOR
            if (DebugManager.Instance.ForceTutoriel)
                OnTutorielSet?.Invoke(true);
#endif
        }

        public void StartLevel()
        {
            m_isChronoStopped = false;
            IsPlaying = true; OnStartLevel.Invoke();
            ChangeMainPlayerState(PlayerState.HUMANOID, PlayersInputManager.Instance.IsSwitched);
            PlanetsCollidingManager.Instance.StartPlanetsMovement(_currentLevel.Times[MedalsType.EARTH]);
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
            if(!IsPaused)
            {
                _savedGameTimeScale = GameTimeScale;
                _savedPausedState = ActivePlayerState;
                IsPaused = true;
                ChangeMainPlayerState(PlayerState.UI, false);
            }
            ModuleManager.Instance.OnModuleEnable(moduleToLoad);
            OnPaused?.Invoke();
        }

        public void Resume()
        {
            IsPlaying = true;
            ChangeMainPlayerState(_savedPausedState, false);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleType.HUD));
            ModuleManager.Instance.ClearNavigationHistoric();
            OnResumed?.Invoke();
        }

        public void EndLevel(bool lose = false)
        {
            if(lose) _timer = float.MaxValue;

            OnPaused?.Invoke();
            CleanInGame(false);
            IsPlaying = false;
            bool newBestTime = false;
            if (!_hasPlayedInSolo && !IsPracticeMode && !lose)
                newBestTime = CurrentLevel.SaveTime(Timer);
            ChangeMainPlayerState(PlayerState.UI, false);
            ModuleManager.Instance.OnModuleEnable(ModuleManager.Instance.GetModule(ModuleType.END_LEVEL));
            ModuleManager.Instance.ClearNavigationHistoric();
            OnEndLevel.Invoke(Timer, newBestTime, _hasPlayedInSolo, IsPracticeMode);
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
                /*
                CharactersManager.Instance.DestroyInstance();
                CameraManager.Instance.DestroyInstance();
                */
            }
            else // Cleann all we need to clean after end level gestion
            {
                _hasPlayedInSolo = false;
                _currentLevel = null;
                m_isChronoStopped = false;
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

        private void TryLoadTutorielData(TutorielData data)
        {
            if (CanOpenPopup)
                LoadTutorielData(data);
        }

        public void LoadTutorielData(TutorielData data)
        {
            ModuleManager.Instance.GetModule(ModuleType.TUTORIEL).GetComponent<TutorielModuleUI>().LoadTutorielData(data);
            Pause(ModuleManager.Instance.GetModule(ModuleType.TUTORIEL));
            ModuleManager.Instance.ClearNavigationHistoric();

            CharactersManager.Instance.CancelAim();
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
            if ((IsPlaying || IsRespawning) && !m_isChronoStopped && !IsPracticeMode)
            {
                float timeDelta = Time.deltaTime * (IsPlaying ? GameTimeScale : _savedGameTimeScale);
                Timer += timeDelta;
                float TimerBeforeCollisionCheck = TimerBeforeCollision - timeDelta;
                TimerBeforeCollision = Mathf.Max(TimerBeforeCollisionCheck,0f);

                OnTimerChanged.Invoke(IsPracticeMode ? Timer : TimerBeforeCollision, IsPracticeMode);
            }
        }

        private bool m_isChronoStopped = false;
        public void StopChrono()
        {
            m_isChronoStopped = true;
            OnTimerChanged.Invoke(-1f, false);
        }

        private const float _outerWildsEasterEggBonus = -0.22f;
        public void ILoveOuterWidls()
        {
            Timer += _outerWildsEasterEggBonus;
            TimerBeforeCollision -= _outerWildsEasterEggBonus;
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
                
                //Slow down music, warn: synced method with CharactersManager and frames, do not make async
                CharactersManager.Instance.SlowMusic();
                
                //Wait until isSlowed becomes false
                yield return new WaitUntil(() => isTimeSlowed == false);

                StopCoroutine(ChangeTimeScale(GameTimeScale, CharactersManager.Instance.GameplayData.TargetAimTimeScale, inDuration));
                GameTimeScale = CharactersManager.Instance.GameplayData.TargetAimTimeScale;
                StartCoroutine(ChangeTimeScale(GameTimeScale, 1f, outDuration));
                GameTimeScale = 1f;
                
                //Speed up music speed, warn: synced method with CharactersManager and frames, do not make async
                CharactersManager.Instance.SpeedUpMusic();

                isSlowMotionSequenceFinished = true;
            }
        }

        IEnumerator ChangeTimeScale(float start, float end, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                GameTimeScale = Mathf.Lerp(start, end, elapsed / duration);
                //Time.fixedDeltaTime = Time.timeScale * 0.02f;// ? que faire
                //Time.timeScale = _gameTimeScale;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            GameTimeScale = end;
        }

        #endregion

        #region Speed lines
        private bool _sizeTweenFinished = false;
        public void ChangeSpeedLines(SpeedLinesState state)
        {
            _sizeTweenFinished = false;
            switch (state)
            {
                case SpeedLinesState.NONE:
                    TweenSpeedLinesSize(1.5f);
                    break;
                case SpeedLinesState.DASH:
                    _speedLines.SetColor("_Color", Color.white);
                    _speedLines.SetFloat("_LineAmount", 15);
                    TweenSpeedLinesSize(_characterGameplayData.DashSize);
                    Invoke("ResetSpeedLines", _characterGameplayData.DashLinesDuration);
                    break;
                case SpeedLinesState.BLACK:
                    _speedLines.SetColor("_Color", _characterGameplayData.BlackManColor);
                    _speedLines.SetFloat("_LineAmount", _characterGameplayData.SinguLineAmount);
                    TweenSpeedLinesSize(_characterGameplayData.SinguSize);
                    break;
                case SpeedLinesState.WHITE:
                    _speedLines.SetColor("_Color", _characterGameplayData.WhiteManColor);
                    _speedLines.SetFloat("_LineAmount", _characterGameplayData.SinguLineAmount);
                    TweenSpeedLinesSize(_characterGameplayData.SinguSize);
                    break;
            }
        }

        public void ApplySpeedLinesSingu(float distance)
        {
            if(_sizeTweenFinished)
            {
                float size = distance * (_characterGameplayData.BaseSize - _characterGameplayData.SinguSize) + _characterGameplayData.SinguSize;
                _speedLines.SetFloat("_Size2", size);
            }
        }

        public void ResetSpeedLines() => ChangeSpeedLines(SpeedLinesState.NONE);

        private void TweenSpeedLinesSize(float newSize)
        {
            Tween tween = DOTween.To(() => _speedLines.GetFloat("_Size2"), x => _speedLines.SetFloat("_Size2", x), newSize, 0.3f);
            tween.onComplete = () => _sizeTweenFinished = true;
            tween.Play();
        }

        #endregion
    }
}
