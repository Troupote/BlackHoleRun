using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.Controls;
using BHR;
using System;
using UnityEngine.Events;
using BHR.UILinkers;
using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class RebindButton : MonoBehaviour
{
    [SerializeField] private InputActionReference[] _actionsToRebind;
    [SerializeField] private bool _isComposite = false;
    [SerializeField, ShowIf(nameof(_isComposite))] private DirectionComposite _directionComposite = DirectionComposite.NONE;
    private InputAction _action;
    private Image _icon;
    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

    private void Awake()
    {
        _icon = transform.GetChild(0).GetComponent<Image>();
        _action = _actionsToRebind[0];
    }

    public void OnLaunchRebinding()
    {
        if(!RebindInputsManager.Instance.IsRebinding)
            StartRebinding();
    }

    private void StartRebinding()
    {
        foreach (var actionRef in _actionsToRebind) actionRef.action.Disable();
        _icon.enabled = false;
        RebindInputsManager.Instance.ToggleRebinding();

        InputDevice deviceRebound = PlayersInputManager.Instance.CurrentAllowedDevice;
        Debug.Log("Start rebinding");

        foreach(InputDevice device in InputSystem.devices.Where(d => d != deviceRebound).ToList())
            if(!KeyboardMouseMatching(device, deviceRebound))
                InputSystem.DisableDevice(device);


        _rebindingOperation = _action.PerformInteractiveRebinding();

        // Set canceler depending on the device type used and binding id (hard coded by convention)
        int bindingId = -1;
        if (deviceRebound is Gamepad)
        {
            _rebindingOperation.WithCancelingThrough("<Gamepad>/start");
            bindingId = 0;
        }
        else if (deviceRebound is Mouse || deviceRebound is Keyboard)
        {
            _rebindingOperation.WithCancelingThrough("<Keyboard>/escape");
            bindingId = 1;
        }
        if (_isComposite && _directionComposite != DirectionComposite.NONE)
            bindingId += (int)_directionComposite;

        // Rebind settings
        _rebindingOperation
            .WithTargetBinding(bindingId)

            .OnPotentialMatch(operation =>
            {
                if (BindingAuthorized(operation.selectedControl))
                        operation.Complete();
                    else
                        operation.Cancel();
            })

            .OnCancel(operation =>
            {
                operation.Dispose();
                Debug.Log("Rebind cancel");
                CleanUp(deviceRebound);
            })

            .OnComplete(operation =>
            {
                DuplicateBinding(bindingId, _action.bindings[bindingId].effectivePath);
                Debug.Log("Rebind successfull. New bindings : " + operation.selectedControl.path);
                operation.Dispose();
                foreach (var actionRef in _actionsToRebind) actionRef.action.Enable();

                _icon.enabled = true;
                GetComponent<RebindInputUILinker>().RegisterSetting(PlayersInputManager.Instance.CurrentAllowedPlayerInput.actions.SaveBindingOverridesAsJson());
                CleanUp(deviceRebound); // Met à jour l’icône selon le nouvel input
            })

            .Start();
    }

    private void CleanUp(InputDevice deviceRebound)
    {
        foreach (InputDevice device in InputSystem.devices.Where(d => d != deviceRebound).ToList())
            if (!KeyboardMouseMatching(device, deviceRebound))
                InputSystem.EnableDevice(device);

        _icon.GetComponent<InputBindingSpriteAutoUI>().UpdateSprite();
        _icon.enabled = true;
        RebindInputsManager.Instance.ToggleRebinding(0.1f);
    }

    private void DuplicateBinding(int bindingIndex, string newPath)
    {
        for(int i=1; i < _actionsToRebind.Length; i++)
        {
            InputAction action = _actionsToRebind[i];
            action.ApplyBindingOverride(bindingIndex, newPath);
        }
    }

    private void OnDisable()
    {
        _rebindingOperation?.Dispose();
    }

    private bool BindingAuthorized(InputControl control)
    {
        //Debug.Log($"Candidat path : {controlPath}, normed in {normedPath}");
        List<string> allPaths = new List<string>();

        // Add normed authorized paths in game to list
        foreach (string path in PlayersInputManager.Instance.ActionsSO.BindingsControlPathToSprite.Keys.ToList())
            allPaths.Add(path);

        // Remove fixed path (pause, sticks...)
        foreach(string fp in _fixedPaths)
            allPaths.Remove(fp);

        // Remove already used path
        foreach(var map in PlayersInputManager.Instance.CurrentAllowedPlayerInput.actions.actionMaps)
            if(map.name == InputActions.SingularityActionMap || map.name == InputActions.HumanoidActionMap)
                foreach (var binding in map.bindings)
                    if (binding.groups.Contains(PlayersInputManager.Instance.CurrentAllowedPlayerInput.currentControlScheme))
                        allPaths.Remove(binding.effectivePath);

        bool authorized = false;
        foreach(string path in allPaths)
            if(InputControlPath.Matches(path, control))
                authorized = true;

        return authorized;
    }

    private string[] _fixedPaths = new string[]
    {
        "<Gamepad>/leftStick", "<Gamepad>/rightStick", "<Gamepad>/start", "<Gamepad>/select",
        "<Keyboard>/r", "<Keyboard>/escape", "<Pointer>/delta", "*/{Menu}"
    };

    private bool KeyboardMouseMatching(InputDevice deviceA, InputDevice deviceB) => deviceA is Mouse && deviceB is Keyboard || deviceA is Keyboard && deviceB is Mouse;
}

