using Sirenix.OdinInspector;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BHR
{
    public class UserSettingsModuleUI : AModuleUI
    {
        [SerializeField, Required, FoldoutGroup("Refs")] private GameObject _controlsPanel;
        [SerializeField, Required, FoldoutGroup("Refs")] private GameObject _advancedPanel;
        [SerializeField, Required, FoldoutGroup("Refs")] private TextMeshProUGUI _activeControllerText;
        [SerializeField, Required, FoldoutGroup("Refs")] private GameObject _switchInfos;
        [SerializeField, Required, FoldoutGroup("Refs")] private Button _switchSettingsButton;
        [SerializeField, Required, FoldoutGroup("Refs")] private GameObject _moveKeyboardInput;
        [SerializeField, Required, FoldoutGroup("Refs")] private GameObject _moveGamepadInput;

        public override void Back()
        {
            SettingsManager.Instance.CancelUserSettings();
            BackAction();
        }

        public void OnApply()
        {
            SettingsManager.Instance.ApplyUserSettings();
            BackAction();
        }

        private void BackAction()
        {
            PlayersInputManager.Instance.AllowOnlyOnePlayerUIInputs(false);
            base.Back();
        }

        private void OnEnable()
        {
            _controlsPanel.SetActive(true);
            _advancedPanel.SetActive(false);
            UpdateControllerInfos(PlayersInputManager.Instance.CurrentAllowedInput);
            PlayersInputManager.Instance.OnAllowedInputChanged.AddListener(UpdateControllerInfos);
            PlayersInputManager.Instance.OnPlayerHasJoined.AddListener(UpdateControllerSwitchInfos);
            PlayersInputManager.Instance.OnPlayerDisconnected.AddListener(UpdateControllerSwitchInfos);
        }

        private void OnDisable()
        {
            PlayersInputManager.Instance.OnAllowedInputChanged.RemoveListener(UpdateControllerInfos);
            PlayersInputManager.Instance.OnPlayerHasJoined.RemoveListener(UpdateControllerSwitchInfos);
            PlayersInputManager.Instance.OnPlayerDisconnected.RemoveListener(UpdateControllerSwitchInfos);
        }

        public void TogglePanels()
        {
            _controlsPanel.SetActive(!_controlsPanel.activeSelf);
            _advancedPanel.SetActive(!_controlsPanel.activeSelf);
            _switchSettingsButton.GetComponentInChildren<TextMeshProUGUI>().text = _controlsPanel.activeSelf ? "Advanced" : "Controls";
        }

        public void ResetUserSettings() => SettingsManager.Instance.ResetUserSettings(PlayersInputManager.Instance.CurrentAllowedDevice);

        private void UpdateControllerInfos(AllowedPlayerInput currentAllowedInput)
        {
            if (currentAllowedInput == AllowedPlayerInput.NONE || currentAllowedInput == AllowedPlayerInput.BOTH)
                return;

            // ControllerText
            PlayerInputController activePlayerInput = PlayersInputManager.Instance.PlayersInputControllerRef[currentAllowedInput == AllowedPlayerInput.FIRST_PLAYER ? 0 : 1];
            string controllersName = "";
            foreach (InputDevice device in activePlayerInput.GetComponent<PlayerInput>().devices)
                controllersName += device.name + ", ";
            controllersName = controllersName.Remove(controllersName.Length - 2);

            _activeControllerText.text = "Current Controller : " + controllersName;

            // Switch input indication if two players
            UpdateControllerSwitchInfos(activePlayerInput.playerIndex);

            // Disable the right move (Gamepad : cannot rebind Move. Keyboard : can rebind it)
            _moveKeyboardInput.SetActive(PlayersInputManager.Instance.CurrentAllowedDevice is Keyboard || PlayersInputManager.Instance.CurrentAllowedDevice is Mouse);
            _moveGamepadInput.SetActive(PlayersInputManager.Instance.CurrentAllowedDevice is Gamepad);
        }

        private void UpdateControllerSwitchInfos(int playerIndex) => _switchInfos.SetActive(PlayersInputManager.Instance.PlayerConnectedCount() >= 2);
    }
}
