using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace BHR
{
    public class UIInputFilterer : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private PlayerInput[] _allowedInput;
        [SerializeField, ReadOnly]
        private List<InputDevice> _allowedDevices = new List<InputDevice>();


        private void OnEnable()
        {
            PlayersInputManager.Instance.OnAllowedInputChanged.AddListener(ChangeAllowedPlayerInput);
            ChangeAllowedPlayerInput(PlayersInputManager.Instance.CurrentAllowedInput);
            InputSystem.onEvent += OnInputEvent;
        }

        private void OnDisable()
        {
            PlayersInputManager.Instance.OnAllowedInputChanged.RemoveListener(ChangeAllowedPlayerInput);
            InputSystem.onEvent -= OnInputEvent;
        }

        private void ChangeAllowedPlayerInput(AllowedPlayerInput newAllowed)
        {
            _allowedInput = new PlayerInput[2];
            _allowedDevices.Clear();

            if ((newAllowed & AllowedPlayerInput.FIRST_PLAYER) != 0)
                _allowedInput[0] = PlayersInputManager.Instance.PlayersInputControllerRef[0].GetComponent<PlayerInput>();
            if ((newAllowed & AllowedPlayerInput.SECOND_PLAYER) != 0)
                _allowedInput[1] = PlayersInputManager.Instance.PlayersInputControllerRef[1].GetComponent<PlayerInput>();

            foreach (PlayerInput playerInputController in _allowedInput)
                if(playerInputController != null)
                    _allowedDevices.AddRange(playerInputController.GetComponent<PlayerInput>().devices);
        }

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            // Si ce device n’est pas dans les devices du joueur autorisé
            if (!_allowedDevices.Contains(device))
            {
                eventPtr.handled = true; // Bloque l’event
            }
        }
    }
}
