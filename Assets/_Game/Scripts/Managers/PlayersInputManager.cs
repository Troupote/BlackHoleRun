using BHR;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace BHR
{
    public class PlayersInputManager : ManagerSingleton<PlayersInputManager>
    {
        [SerializeField, Required] private InputActionsSO _actionsSO;
        public InputActionsSO ActionsSO => _actionsSO;

        [SerializeField, ReadOnly] private bool _canConnect; // Enable the ControllerSelection 
        public bool CanConnect { get => _canConnect; set => _canConnect = value; }

        [SerializeField, ReadOnly] private bool _reconnecting = false;

        // Players managing
        private PlayerInputController[] _playersInputControllerRef = new PlayerInputController[2];
        public PlayerInputController[] PlayersInputControllerRef { get => _playersInputControllerRef; set { _playersInputControllerRef = value;} }

        [SerializeField, ReadOnly]
        private AllowedPlayerInput _currentAllowedInput = AllowedPlayerInput.BOTH;
        public AllowedPlayerInput CurrentAllowedInput
        {
            get => _currentAllowedInput;
            set
            {
                _currentAllowedInput = value;
                OnAllowedInputChanged.Invoke(_currentAllowedInput);
            }
        }

        public InputDevice CurrentAllowedDevice => CurrentAllowedPlayerInput?.devices[0];

        public InputDevice CurrentActivePlayerDevice => CurrentActivePlayerInput.devices[0];
        public PlayerControllerState CurrentActiveControllerState => PlayersControllerState[GameManager.Instance.ActivePlayerIndex];

        public PlayerInput CurrentActivePlayerInput => PlayersInputControllerRef[GameManager.Instance.ActivePlayerIndex].GetComponent<PlayerInput>();
        public PlayerInput CurrentAllowedPlayerInput => CurrentAllowedInput switch
        {
            AllowedPlayerInput.FIRST_PLAYER => PlayersInputControllerRef[0].GetComponent<PlayerInput>(),
            AllowedPlayerInput.SECOND_PLAYER => PlayersInputControllerRef[1].GetComponent<PlayerInput>(),
            _ => null
        };

        private bool _isOnlyOnePlayerInputsAllowed = false;

#if UNITY_EDITOR
        [Button] private void ForceAllowedInputState(AllowedPlayerInput state) => CurrentAllowedInput = state;
#endif

        public int LastPlayerIndexUIInput;

        [SerializeField, ReadOnly] private PlayerControllerState[] _playersControllerState;
        public PlayerControllerState[] PlayersControllerState => _playersControllerState;
        [SerializeField, ReadOnly] private PlayerReadyState[] _playersReadyState;
        public PlayerReadyState[] PlayersReadyState => _playersReadyState;

        [SerializeField, ReadOnly]
        private bool _soloPlayer = false;
        public bool SoloPlayer
        {
            get => _soloPlayer;
            private set => _soloPlayer = value;
        }
        [SerializeField, ReadOnly]
        private bool _soloModeEnabled = false;
        public bool SoloModeEnabled
        {
            get => _soloModeEnabled;
            set { _soloModeEnabled = value; OnSoloModeToggle.Invoke(SoloModeEnabled); }
        }
        private bool _isSwitched = false;
        public bool IsSwitched
        {
            get => _isSwitched;
            private set { _isSwitched = value; OnPlayersSwitch.Invoke(IsSwitched); }
        }

        public UnityEvent<AllowedPlayerInput> OnAllowedInputChanged;

        public UnityEvent<InputAction.CallbackContext> OnUIInput;
        public UnityEvent<InputAction.CallbackContext, int> OnInput;

        public UnityEvent<int> OnPlayerHasJoined;
        public UnityEvent<int> OnPlayerDisconnected;
        public UnityEvent<PlayerReadyState, int> OnPlayerReadyStateChanged;
        public UnityEvent<bool> OnSoloModeToggle;
        public UnityEvent<bool> OnPlayersSwitch;

        public override void Awake()
        {
            base.Awake();   
            Init();
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void Init()
        {
            _playersControllerState = new PlayerControllerState[] { PlayerControllerState.DISCONNECTED, PlayerControllerState.DISCONNECTED };
            _playersReadyState = new PlayerReadyState[] { PlayerReadyState.NONE, PlayerReadyState.NONE };
            InputActions.Initialize(ActionsSO);
        }

        private void Start()
        {
            ModuleManager.Instance.OnModuleEnabled.AddListener(OnModuleEnabled);
            OnIReconnect?.AddListener(AttemptReconnection);
        }

        #region Input management
        public void HandleInput(InputAction.CallbackContext ctx, int playerIndex)
        {
            if(RebindInputsManager.Instance.IsRebinding)
                return;

            string actionMap = ctx.action.actionMap.name;
            //Debug.Log($"Player in {actionMap} state has {ctx.phase} {ctx.action.name} with {ctx.control.device.name}");

            // Specific action common to all actions maps (except Empty)
            if (ctx.action.name == InputActions.Pause)
            {
                OnPause.Invoke();
                return;
            }

            if (actionMap == InputActions.UIActionMap)
            {
                // Specific case in Player selection where while in UI we need to differenciate players, but not with the map, with the playerIndex.
                if(ModuleManager.Instance.ModulesRef[ModuleManager.Instance.CurrentModule] == ModuleType.PLAYER_SELECTION)
                    ControllerSelectionHandleInput(ctx, playerIndex);
                // UI input are listened by every subbed modules
                else
                    OnUIInput.Invoke(ctx);
            }
            else // In-Game action map -> PlayerInputReceiver
            {
                SendInputEvent(ctx, playerIndex);
            }

            // For debug purpose or Game Manager, there's this generic input event
            OnInput.Invoke(ctx, playerIndex);
        }

        public UnityEvent OnHJump, OnHDash, OnHSlide, OnHThrow, OnSJump, OnSDash, OnSUnmorph, OnPause, OnIReconnect; // Performed events
        public UnityEvent<Vector2> OnHMove, OnSMove; // Vector2 value events
        public UnityEvent<Vector2, PlayerControllerState> OnHLook, OnSLook; // Vector2 value events depending of the controller used
        public UnityEvent<bool> OnHAim; // Multiple callbacks events (Hold throw, Hold/toggle aim)
        public UnityEvent OnRestartLevel, OnRespawn; // Interactions performed events

        private void SendInputEvent(InputAction.CallbackContext ctx, int playerIndex)
        {
            // Get controller type used 
            PlayerControllerState controller = PlayersControllerState[playerIndex];

            // If IsPlaying only -> don't know if the best is to block the input here or to block the movements everywhere else
            if (!GameManager.Instance.IsPlaying)
                return;

            #region Common at all player states
            if (ctx.action.name == InputActions.Restart && ctx.performed)
                if (ctx.interaction is HoldInteraction)
                    OnRestartLevel.Invoke();
                else if(ctx.interaction is MultiTapInteraction)
                    OnRespawn.Invoke();
            #endregion

            #region Specific player state
            // HUMANOID
            if (ctx.action.actionMap.name == InputActions.HumanoidActionMap)
            {
                if (ctx.action.name == InputActions.Look)
                {
                    Vector2 value = ctx.ReadValue<Vector2>();

                    // Deadzone check 
                    if (controller == PlayerControllerState.GAMEPAD && value.magnitude <= SettingsSave.LoadRightStickDeadzone(CurrentActivePlayerDevice))
                        value = Vector2.zero;

                    // Invert axe Y check
                    if (SettingsSave.LoadInvertAxeY(CurrentActivePlayerDevice))
                        value = new Vector2(value.x, -value.y);

                    OnHLook.Invoke(value, controller);
                }

                else if (ctx.action.name == InputActions.Move)
                {
                    Vector2 value = ctx.ReadValue<Vector2>();

                    // Deadzone check 
                    if (controller == PlayerControllerState.GAMEPAD && value.magnitude <= SettingsSave.LoadLeftStickDeadzone(CurrentActivePlayerDevice))
                        value = Vector2.zero;

                    OnHMove.Invoke(value);
                }

                else if (ctx.action.name == InputActions.Aim && (ctx.performed || ctx.canceled && SettingsSave.LoadToggleAim(CurrentActivePlayerDevice) == 0) || ctx.action.name == InputActions.Throw && ctx.canceled)
                    OnHAim.Invoke(ctx.action.name == InputActions.Throw && ctx.canceled);

                if (ctx.performed)
                {
                    if (ctx.action.name == InputActions.Jump)
                        OnHJump.Invoke();

                    else if (ctx.action.name == InputActions.Dash)
                        OnHDash.Invoke();

                    //else if (ctx.action.name == InputActions.Slide)
                    //    OnHSlide.Invoke();

                    else if (ctx.action.name == InputActions.Throw)
                        OnHThrow.Invoke();
                }
            }

            // SINGULARITY
            else if (ctx.action.actionMap.name == InputActions.SingularityActionMap)
            {
                if (ctx.action.name == InputActions.Look)
                {
                    Vector2 value = ctx.ReadValue<Vector2>();

                    // Deadzone check 
                    if (controller == PlayerControllerState.GAMEPAD && value.magnitude <= SettingsSave.LoadRightStickDeadzone(CurrentActivePlayerDevice))
                        return;

                    // Invert axe Y check
                    if (SettingsSave.LoadInvertAxeY(CurrentActivePlayerDevice))
                        value = new Vector2(value.x, -value.y);

                    OnSLook.Invoke(value, controller);
                }

                else if (ctx.action.name == InputActions.Move)
                {
                    Vector2 value = ctx.ReadValue<Vector2>();

                    // Deadzone check 
                    if (controller == PlayerControllerState.GAMEPAD && value.magnitude <= SettingsSave.LoadLeftStickDeadzone(CurrentActivePlayerDevice))
                        return;

                    OnSMove.Invoke(value);
                }

                if (ctx.performed)
                {
                    if (ctx.action.name == InputActions.Jump)
                        OnSJump.Invoke(); // @todo link to singularity jump action

                    else if (ctx.action.name == InputActions.Dash)
                        OnSDash.Invoke(); // @todo link to singularity dash action

                    //else if (ctx.action.name == InputActions.Unmorph)
                    //    OnSUnmorph.Invoke(); // @todo link to singularity unmorph action (if any)
                }
            }

            // INACTIVE
            else if (ctx.action.actionMap.name == InputActions.InactiveActionMap)
                if (ctx.performed && ctx.action.name == InputActions.Reconnect)
                    OnIReconnect.Invoke();
            #endregion
        }

        private void SetAllowedInput(int playerIndex, bool disconnecting)
        {
            //if (ModuleManager.Instance.CurrentModule != ModuleManager.Instance.GetModule(ModuleType.MAP_REBINDING))
            {
                if(PlayerConnectedCount() == 0)
                    CurrentAllowedInput = AllowedPlayerInput.NONE;
                else if (PlayerConnectedCount() == 1 || _isOnlyOnePlayerInputsAllowed)
                    CurrentAllowedInput = playerIndex == 0 || disconnecting ? AllowedPlayerInput.FIRST_PLAYER : AllowedPlayerInput.SECOND_PLAYER;
                else if (PlayerConnectedCount() == 2)
                    CurrentAllowedInput = AllowedPlayerInput.BOTH;
            }
        }

        public void AllowOnlyOnePlayerUIInputs(bool enable)
        {
            if(enable)
            {
                CurrentAllowedInput = LastPlayerIndexUIInput == 0 || PlayerConnectedCount() <= 1 ? AllowedPlayerInput.FIRST_PLAYER : AllowedPlayerInput.SECOND_PLAYER;
            }
            else
            {
                CurrentAllowedInput = PlayerConnectedCount() <= 1 ? AllowedPlayerInput.FIRST_PLAYER : AllowedPlayerInput.BOTH;
            }
            _isOnlyOnePlayerInputsAllowed = enable;
        }

        public void ToggleCurrentAllowedInput()
        {
            if (CurrentAllowedInput == AllowedPlayerInput.NONE || CurrentAllowedInput == AllowedPlayerInput.BOTH || PlayerConnectedCount() <= 1)
                return;

            CurrentAllowedInput = CurrentAllowedInput == AllowedPlayerInput.FIRST_PLAYER ? AllowedPlayerInput.SECOND_PLAYER : AllowedPlayerInput.FIRST_PLAYER;
        }

        #endregion

        #region Connect and disconncect gestion
        public void OnPlayerJoined(PlayerInput playerInput)
        {
            // Resolve switch bug
            RemoveSwitchXInput(playerInput.devices[0]);

            if(PlayerConnectedCount()>=2 || !AssignPlayerIndex(playerInput)) // Max players connected at the same time
            {
                Destroy(playerInput.gameObject);
                return;
            }

            PlayerInputController playerInputController = playerInput.GetComponent<PlayerInputController>();
            Debug.Log($"Player {playerInputController.playerIndex} joined !\nController : {playerInput.devices[0]} (Scheme : {playerInput.currentControlScheme})\nAction map : {playerInput.currentActionMap.name}");


            playerInputController.transform.SetParent(transform);

            UpdatePlayerControllerState(playerInputController.playerIndex);
            UpdatePlayerReadyState(playerInputController.playerIndex, PlayerReadyState.CONNECTED);

            SetSoloPlayer();
            SoloModeEnabled = false;

            SetAllowedInput(playerInputController.playerIndex, false);

            if(GameManager.Instance.SoloMode && ( GameManager.Instance.IsPlaying || GameManager.Instance.IsPaused))
                Reconnection();

            OnPlayerHasJoined.Invoke(playerInputController.playerIndex);
        }

        private bool AssignPlayerIndex(PlayerInput playerInput)
        {
            PlayerInputController control = playerInput.GetComponent<PlayerInputController>();
            int freeIndex = -1;
            if (PlayersInputControllerRef[0] == null) freeIndex = 0;
            else if (PlayersInputControllerRef[1] == null) freeIndex = 1;

            // Override controller logged out
            if(PlayersReadyState.Contains(PlayerReadyState.LOGGED_OUT))
            {
                int loggedOutIndex = Array.IndexOf(PlayersReadyState, PlayerReadyState.LOGGED_OUT);
                Destroy(PlayersInputControllerRef[loggedOutIndex].gameObject);
                freeIndex = loggedOutIndex;
            }

            if (freeIndex == -1)
            {
                Debug.Log("All players index are full");
                return false;
            }
            else
            {
                PlayersInputControllerRef[freeIndex] = control;
                control.playerIndex = freeIndex;
                return true;
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Disconnected)
            {
                Debug.Log(device.name + " is disconnecting");
                foreach (PlayerInputController playerInputController in PlayersInputControllerRef)
                {
                    if (playerInputController != null && playerInputController.GetComponent<PlayerInput>().devices.Count == 0) // Found the player ref that disconnected
                    {
                        OnPlayerDisconnecting(playerInputController);
                    }
                }
            }
        }

        private void OnPlayerDisconnecting(PlayerInputController playerInputController)
        {
            // Check if player was playing
            if(PlayersReadyState[playerInputController.playerIndex] == PlayerReadyState.READY && GameManager.Instance.IsPlaying)
                Reconnection();

            UpdatePlayerControllerState(playerInputController.playerIndex);
            UpdatePlayerReadyState(playerInputController.playerIndex, PlayerReadyState.NONE);
            PlayersInputControllerRef[playerInputController.playerIndex] = null;
            SetSoloPlayer();
            CheckReadyState();

            SetAllowedInput(playerInputController.playerIndex, true);

            Debug.Log($"Player {playerInputController.playerIndex} is disconnecting");
            OnPlayerDisconnected.Invoke(playerInputController.playerIndex);
            Destroy(playerInputController.gameObject);

        }
        #endregion

        #region ControllerSelection
        private void OnModuleEnabled(GameObject module, bool back)
        {
            if (ModuleManager.Instance.ModulesRef[module] == ModuleType.PLAYER_SELECTION)
                OnPlayerSelectionEnable();
        }

        private void OnPlayerSelectionEnable()
        {
            CanConnect = true;

            // Default assignation
            for(int i=0; i < PlayersControllerState.Length; i++)
                if(PlayersControllerState[i] != PlayerControllerState.DISCONNECTED)
                    UpdatePlayerReadyState(i, PlayerReadyState.CONNECTED);

            SetSoloPlayer();
        }

        private void AttemptReconnection()
        {
            if (GameManager.Instance.SoloMode && GameManager.Instance.IsPlaying)
                Reconnection();
        }

        private void Reconnection()
        {
            _reconnecting = true;
            GameManager.Instance.Pause(ModuleManager.Instance.GetModule(ModuleType.PLAYER_SELECTION));
        }

        private void UpdatePlayerControllerState(int playerIndex)
        {
            PlayerControllerState state = PlayerControllerState.DISCONNECTED;
            if (PlayersInputControllerRef[playerIndex].GetComponent<PlayerInput>().devices.Count > 0)
            {
                if (PlayersInputControllerRef[playerIndex].GetComponent<PlayerInput>().currentControlScheme == InputActions.KeyboardScheme)
                    state = PlayerControllerState.KEYBOARD;
                else if(PlayersInputControllerRef[playerIndex].GetComponent<PlayerInput>().currentControlScheme == InputActions.GamepadScheme)
                    state = PlayerControllerState.GAMEPAD;
            }

            PlayersControllerState[playerIndex] = state;
        }

        private void UpdatePlayerReadyState(int playerIndex,PlayerReadyState state)
        {
            PlayersReadyState[playerIndex] = state;
            OnPlayerReadyStateChanged.Invoke(state, playerIndex);
        }

        private void ControllerSelectionHandleInput(InputAction.CallbackContext ctx, int playerIndex)
        {
            if(ctx.performed)
            {
                if (ctx.action.name == InputActions.Submit)
                {
                    OnSubmitAction(playerIndex);
                }
                else if(ctx.action.name == InputActions.Cancel)
                {
                    OnCancelAction(playerIndex);
                }
                else if(ctx.action.name == InputActions.Rebind)
                {
                    OnRebindAction(playerIndex);
                }
                else if(ctx.action.name == InputActions.Switch)
                {
                    OnSwitchAction();
                }
            }
        }

        public void OnSubmitAction(int playerIndex)
        {
            // Toggle ready state (except if SoloPlayer plays in solo mode, that's why there's this if condition)
            if (!SoloPlayer || !SoloModeEnabled || PlayersReadyState[playerIndex] != PlayerReadyState.READY)
            {
                if (SoloModeEnabled) SoloModeEnabled = false;
                bool connecting = PlayersReadyState[playerIndex] == PlayerReadyState.READY || PlayersReadyState[playerIndex] == PlayerReadyState.LOGGED_OUT || PlayersReadyState[playerIndex] == PlayerReadyState.NONE;
                UpdatePlayerReadyState(playerIndex, connecting ? PlayerReadyState.CONNECTED : PlayerReadyState.READY);
            }

            SetSoloPlayer();

            if (CheckReadyState())
            {
                if (_reconnecting)
                {
                    _reconnecting = false;
                    SetSoloPlayer();
                    GameManager.Instance.SoloMode = SoloPlayer;
                    GameManager.Instance.Resume();
                }
                else
                    GameManager.Instance.LaunchLevel(!GameManager.Instance.IsPaused);
            }
        }

        public void OnCancelAction(int playerIndex)
        {
            // Back action if already disconnected 
            if (PlayersReadyState[playerIndex] == PlayerReadyState.LOGGED_OUT)
            {
                if (PlayerConnectedCount() <= 1)
                    return; 

                if (_reconnecting)
                {
                    _reconnecting = false;
                    SetSoloPlayer();
                    GameManager.Instance.SoloMode = SoloPlayer;
                    GameManager.Instance.Resume();
                    return;
                }
                else
                    ModuleManager.Instance.Back();
            }

            // Cancel ready state or log out
            UpdatePlayerReadyState(playerIndex, PlayersReadyState[playerIndex] == PlayerReadyState.READY ? PlayerReadyState.CONNECTED : PlayerReadyState.LOGGED_OUT);


            SetSoloPlayer();

            // Cancel SoloMode if enabled and recheck
            if (SoloModeEnabled) SoloModeEnabled = false;
            CheckReadyState();
        }

        public void OnRebindAction(int playerIndex)
        {
            ModuleManager.Instance.OnModuleEnableWithTransition(ModuleManager.Instance.GetModule(ModuleType.MAP_REBINDING));
            AllowOnlyOnePlayerUIInputs(true);
        }

        private void OnSwitchAction()
        {
            if (!SoloModeEnabled && PlayerConnectedCount() > 0)
                IsSwitched = !IsSwitched;
        }

        public int PlayerReadyCount() => PlayersReadyState.Where(r => r == PlayerReadyState.READY).ToList().Count;
        public int PlayerConnectedCount() => PlayersReadyState.Where(r => r == PlayerReadyState.CONNECTED).ToList().Count + PlayerReadyCount();

        public void SetSoloPlayer()
        {
            SoloPlayer = PlayersReadyState.Contains(PlayerReadyState.LOGGED_OUT) || PlayersReadyState.Contains(PlayerReadyState.NONE);
        }

        private bool CheckReadyState()
        {
            if (PlayerReadyCount() == 1 && SoloPlayer)
            {
                if (SoloModeEnabled)
                    return true;
                else
                    SoloModeEnabled = true;
            }
            return PlayerReadyCount() == 2;
        }
        #endregion

        #region Hard Fix Switch twice controllers bug
        private void RemoveSwitchXInput(InputDevice device)
        {
            if (IsUnwantedXInput(device))
            {
                Debug.LogWarning($"Removing duplicate XInput device: {device.displayName}");
                InputSystem.RemoveDevice(device);
            }
        }

        bool IsUnwantedXInput(InputDevice device)
        {
            var desc = device.description;
            return desc.interfaceName == "XInput" &&
                   !desc.product.ToLower().Contains("xbox");
        }
        #endregion
    }

}
