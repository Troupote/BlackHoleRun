using BHR;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [ReadOnly]
    public int playerIndex = -1;

    private PlayerInput _playerInput;
    [SerializeField, ReadOnly]
    private PlayerState _playerState;
    public PlayerState PlayerState
    {
        get => _playerState;
        set
        {
            _playerState = value;
            PlayerStateChanged(_playerState);
        }
    }

#if UNITY_EDITOR
    [Button]
    private void ForcePlayerState(PlayerState state) => PlayerState = state;
#endif

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        ApplyRebinding(SettingsSave.LoadBindings(_playerInput.devices[0]));
        SettingsManager.Instance.OnUserBindingsLoaded.AddListener(ApplyRebinding);
        SettingsManager.Instance.OnUserSettingsCanceled.AddListener(() => ApplyRebinding(SettingsSave.LoadBindings(_playerInput.devices[0])));
    }

    IEnumerator InactiveOnJoinDelay(PlayerState state, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayerState = state;
        LinkActions();
    }

    private void OnEnable()
    {
        PlayerState state = PlayersInputManager.Instance.CanConnect ? GameManager.Instance.ActivePlayerState : PlayerState.NONE;
        PlayersInputManager.Instance.SetSoloPlayer();
        StartCoroutine(InactiveOnJoinDelay(state, 0.2f));
    }
    
    private void LinkActions()
    {
        foreach(var map in _playerInput.actions.actionMaps)
        {
            foreach(var action in map.actions)
            {
                action.started += HandleInput;
                action.performed += HandleInput;
                action.canceled += HandleInput;
            }
        }
    }

    private void ApplyRebinding(string jsonBindings)
    {
        if (jsonBindings == SettingsSave.DEFAULT_BINDINGS)
            _playerInput.actions.RemoveAllBindingOverrides();
        else
            _playerInput.actions.LoadBindingOverridesFromJson(jsonBindings);
    }

    private void HandleInput(InputAction.CallbackContext ctx)
    {
        if(ctx.action.actionMap == _playerInput.currentActionMap)
            PlayersInputManager.Instance.HandleInput(ctx, playerIndex);
    }

    private void PlayerStateChanged(PlayerState state)
    {
        if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();
        //Debug.Log($"Changing state of player {_playerInput.playerIndex} to {state}");
        switch(state)
        {
            case PlayerState.UI: 
                _playerInput.SwitchCurrentActionMap(InputActions.UIActionMap); 
                break;
            case PlayerState.HUMANOID:
                _playerInput.SwitchCurrentActionMap(InputActions.HumanoidActionMap);
                break;
            case PlayerState.SINGULARITY:
                _playerInput.SwitchCurrentActionMap(InputActions.SingularityActionMap);
                break;
            case PlayerState.INACTIVE:
                _playerInput.SwitchCurrentActionMap(InputActions.InactiveActionMap);
                break;
            case PlayerState.NONE:
                _playerInput.SwitchCurrentActionMap(InputActions.EmptyActionMap);
                    break;
        }
    }
}
