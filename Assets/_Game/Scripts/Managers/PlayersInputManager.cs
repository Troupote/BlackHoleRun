using BHR;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BHR
{
    public class PlayersInputManager : ManagerSingleton<PlayersInputManager>
    {
        [SerializeField, Required] private InputActionsSO Actions;

        [SerializeField, ReadOnly] private bool _canConnect; // Enable the ControllerSelection 
        public bool CanConnect { get => _canConnect; set => _canConnect = value; }

        // Players managing
        private PlayerInputController[] _playersInputControllerRef = new PlayerInputController[2];
        public PlayerInputController[] PlayersInputControllerRef { get => _playersInputControllerRef; set { _playersInputControllerRef = value;} }
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

        public UnityEvent<InputAction.CallbackContext> OnUIInput;
        public UnityEvent<InputAction.CallbackContext> OnInput;

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
            InputActions.Initialize(Actions);
        }

        private void Start()
        {
            ModuleManager.Instance.OnModuleEnabled.AddListener(OnModuleEnabled);
        }

        #region Input management
        public void HandleInput(InputAction.CallbackContext ctx, int playerIndex)
        {
            string actionMap = ctx.action.actionMap.name;
            //Debug.Log($"Player in {actionMap} state has {ctx.phase} {ctx.action.name} with {ctx.control.device.name}");

            if (actionMap == InputActions.UIActionMap)
            {
                // Specific case in Player selection where while in UI we need to differenciate players, but not with the map, with the playerIndex.
                if(ModuleManager.Instance.ModulesRef[ModuleManager.Instance.CurrentModule] == ModuleManager.ModuleType.PLAYER_SELECTION)
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
            OnInput.Invoke(ctx);
        }

        public UnityEvent OnHJump, OnHDash, OnHSlide, OnHThrow, OnSJump, OnSDash, OnSUnmorph, OnPause; // Performed events
        public UnityEvent<Vector2> OnHMove, OnSMove; // Vector2 value events
        public UnityEvent<Vector2, PlayerControllerState> OnHLook, OnSLook; // Vector2 value events depending of the controller used
        public UnityEvent OnHAim; // Hold events

        private void SendInputEvent(InputAction.CallbackContext ctx, int playerIndex)
        {
            // Get controller type used 
            PlayerControllerState controller = PlayersControllerState[playerIndex];

            // Specific global actions (no need to diferentiate players)
            if (ctx.performed && ctx.action.name == InputActions.Pause)
                OnPause.Invoke();

            // Character actions in game (need to diferentiate players)

            // If IsPlaying only -> don't know if the best is to block the input here or to block the movements everywhere else
            if (!GameManager.Instance.IsPlaying)
                return;

            // HUMANOID
            if (ctx.action.actionMap.name == InputActions.HumanoidActionMap)
            {
                if (ctx.action.name == InputActions.Look)
                {
                    Vector2 value = ctx.ReadValue<Vector2>();

                    // Deadzone check 
                    if (controller == PlayerControllerState.GAMEPAD && value.magnitude <= SettingsSave.LoadRightStickDeadzone(playerIndex))
                        value = Vector2.zero;

                    OnHLook.Invoke(value, controller);
                }

                else if (ctx.action.name == InputActions.Move)
                {
                    Vector2 value = ctx.ReadValue<Vector2>();

                    // Deadzone check 
                    if (controller == PlayerControllerState.GAMEPAD && value.magnitude <= SettingsSave.LoadLeftStickDeadzone(playerIndex))
                        value = Vector2.zero;

                    OnHMove.Invoke(value);
                }

                else if (ctx.action.name == InputActions.Aim)
                    OnHAim.Invoke();

                if (ctx.performed)
                {
                    if (ctx.action.name == InputActions.Jump)
                        OnHJump.Invoke();

                    else if (ctx.action.name == InputActions.Dash)
                        OnHDash.Invoke();

                    else if (ctx.action.name == InputActions.Slide)
                        OnHSlide.Invoke();

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
                    if (controller == PlayerControllerState.GAMEPAD && value.magnitude <= SettingsSave.LoadRightStickDeadzone(playerIndex))
                        return;

                    OnSLook.Invoke(value, controller);
                }

                else if (ctx.action.name == InputActions.Move)
                {
                    Vector2 value = ctx.ReadValue<Vector2>();

                    // Deadzone check 
                    if (controller == PlayerControllerState.GAMEPAD && value.magnitude <= SettingsSave.LoadLeftStickDeadzone(playerIndex))
                        return;

                    OnSMove.Invoke(value);
                }

                if (ctx.performed)
                {
                    if (ctx.action.name == InputActions.Jump)
                        OnSJump.Invoke(); // @todo link to singularity jump action

                    else if (ctx.action.name == InputActions.Dash)
                        OnSDash.Invoke(); // @todo link to singularity dash action

                    else if (ctx.action.name == InputActions.Unmorph)
                        OnSUnmorph.Invoke(); // @todo link to singularity unmorph action (if any)
                }
            }
        }


        #endregion

        #region Connect and disconncect gestion
        public void OnPlayerJoined(PlayerInput playerInput)
        {
            AssignPlayerIndex(playerInput);
            PlayerInputController playerInputController = playerInput.GetComponent<PlayerInputController>();
            Debug.Log($"Player {playerInputController.playerIndex} joined !\nController : {playerInput.devices[0]} (Scheme : {playerInput.currentControlScheme})\nAction map : {playerInput.currentActionMap.name}");

            playerInputController.transform.SetParent(transform);

            UpdatePlayerControllerState(playerInputController.playerIndex);
            UpdatePlayerReadyState(playerInputController.playerIndex, PlayerReadyState.CONNECTED);


            SetSoloPlayer();
            SoloModeEnabled = false;

            // Resolve switch bug
            RemoveSwitchXInput(playerInput.devices[0]);
        }

        private void AssignPlayerIndex(PlayerInput playerInput)
        {
            PlayerInputController control = playerInput.GetComponent<PlayerInputController>();
            int freeIndex = -1;
            if (PlayersInputControllerRef[0] == null) freeIndex = 0;
            else if (PlayersInputControllerRef[1] == null) freeIndex = 1;

            if (freeIndex == -1)
                Debug.Log("All players index are full");
            else
            {
                PlayersInputControllerRef[freeIndex] = control;
                control.playerIndex = freeIndex;
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Disconnected)
            {
                foreach (PlayerInputController playerInputController in PlayersInputControllerRef)
                {
                    if (playerInputController != null && playerInputController.GetComponent<PlayerInput>().devices.Count == 0)
                    {
                        UpdatePlayerControllerState(playerInputController.playerIndex);
                        UpdatePlayerReadyState(playerInputController.playerIndex, PlayerReadyState.NONE);
                        PlayersInputControllerRef[playerInputController.playerIndex] = null;
                        SetSoloPlayer();
                        CheckReadyState();
                        Debug.Log($"Player {playerInputController.playerIndex} is deconnecting because his {device.name} controller deconnected");
                        Destroy(playerInputController.gameObject);
                    }
                }
            }
        }
        #endregion

        #region ControllerSelection
        private void OnModuleEnabled(GameObject module, bool back)
        {
            if (ModuleManager.Instance.ModulesRef[module] == ModuleManager.ModuleType.PLAYER_SELECTION)
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
                GameManager.Instance.SoloMode = SoloModeEnabled;
                GameManager.Instance.LaunchLevel();
            }
        }

        public void OnCancelAction(int playerIndex)
        {
            // Back action if already disconnected 
            if (PlayersReadyState[playerIndex] == PlayerReadyState.LOGGED_OUT)
                ModuleManager.Instance.Back();

            // Cancel ready state or log out
            UpdatePlayerReadyState(playerIndex, PlayersReadyState[playerIndex] == PlayerReadyState.READY ? PlayerReadyState.CONNECTED : PlayerReadyState.LOGGED_OUT);


            SetSoloPlayer();

            // Cancel SoloMode if enabled and recheck
            if (SoloModeEnabled) SoloModeEnabled = false;
            CheckReadyState();
        }

        public void OnRebindAction(int playerIndex)
        {
            Debug.Log("Rebind stuff");
            //@todo Rebinding
        }

        private void OnSwitchAction()
        {
            if (!SoloModeEnabled && PlayerConnectedCount() > 0)
                IsSwitched = !IsSwitched;
        }

        public int PlayerReadyCount() => PlayersReadyState.Where(r => r == PlayerReadyState.READY).ToList().Count;
        public int PlayerConnectedCount() => PlayersReadyState.Where(r => r != PlayerReadyState.LOGGED_OUT && r != PlayerReadyState.NONE).ToList().Count;

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
            if (device is Gamepad gamepad && IsUnwantedXInput(gamepad))
            {
                Debug.LogWarning($"Removing duplicate XInput device: {device.displayName}");
                InputSystem.RemoveDevice(device);
            }
        }

        bool IsUnwantedXInput(Gamepad gamepad)
        {
            var desc = gamepad.description;
            return desc.interfaceName == "XInput" &&
                   !desc.product.ToLower().Contains("xbox");
        }
        #endregion
    }

}
