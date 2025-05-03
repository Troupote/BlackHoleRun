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
            InputSystem.onEvent += OnInputEvent;
            PlayersInputManager.Instance.OnAllowedInputChanged.AddListener(ChangeAllowedPlayerInput);
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

            PlayerInputController[] picr = PlayersInputManager.Instance.PlayersInputControllerRef;
            PlayerInput[] pir = new PlayerInput[] { picr[0]?.GetComponent<PlayerInput>(),picr[1]?.GetComponent<PlayerInput>() };

            _allowedInput = newAllowed switch
            {
                AllowedPlayerInput.FIRST_PLAYER => new PlayerInput[] { pir[0]},
                AllowedPlayerInput.SECOND_PLAYER => new PlayerInput[] { pir[1] },
                AllowedPlayerInput.BOTH => pir,
                _ => new PlayerInput[] { null, null }
            };

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
                return;
            }

            PlayersInputManager.Instance.LastPlayerIndexUIInput = _allowedInput.First(i => i.GetComponent<PlayerInput>().devices.Contains(device)).playerIndex;
        }
    }
}
