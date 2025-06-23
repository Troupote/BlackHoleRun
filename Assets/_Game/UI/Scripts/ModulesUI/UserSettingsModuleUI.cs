using Assets.SimpleLocalization.Scripts;
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
        [SerializeField, Required, FoldoutGroup("Localization")] private string _controlsKey;
        [SerializeField, Required, FoldoutGroup("Localization")] private string _advancedKey;
        [SerializeField, Required, FoldoutGroup("Localization")] private string _currentControllerKey;
        [SerializeField, Required, FoldoutGroup("Localization")] private string _mouseKey;
        [SerializeField, Required, FoldoutGroup("Localization")] private string _keyboardKey;
        [SerializeField, Required, FoldoutGroup("Localization")] private string _controllerKey;
        [SerializeField, Required, FoldoutGroup("Localization")] private string _gamepadKey;

        public override void Back()
        {
            if (RebindInputsManager.Instance.IsRebinding)
                return;

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
            TogglePanels(false);
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
            bool changeToAdvanced = _controlsPanel.activeSelf;
            _advancedPanel.SetActive(changeToAdvanced);
            _controlsPanel.SetActive(!changeToAdvanced);
            _switchSettingsButton.GetComponentInChildren<TextMeshProUGUI>().text = !changeToAdvanced ? LocalizationManager.Localize(_advancedKey) : LocalizationManager.Localize(_controlsKey);
        }

        public void TogglePanels(bool advanced)
        {
            _controlsPanel.SetActive(!advanced);
            _advancedPanel.SetActive(advanced);
            _switchSettingsButton.GetComponentInChildren<TextMeshProUGUI>().text = _controlsPanel.activeSelf ? LocalizationManager.Localize(_advancedKey) : LocalizationManager.Localize(_controlsKey);
        }

        public void ResetUserSettings()
        {
            if (_controlsPanel.activeSelf)
                SettingsManager.Instance.ResetBindingsSettings(PlayersInputManager.Instance.CurrentAllowedDevice);
            else
                SettingsManager.Instance.ResetAdvancedUserSettings(PlayersInputManager.Instance.CurrentAllowedDevice);
        }

        private void UpdateControllerInfos(AllowedPlayerInput currentAllowedInput)
        {
            if (currentAllowedInput == AllowedPlayerInput.NONE || currentAllowedInput == AllowedPlayerInput.BOTH)
                return;

            // ControllerText
            PlayerInputController activePlayerInput = PlayersInputManager.Instance.PlayersInputControllerRef[currentAllowedInput == AllowedPlayerInput.FIRST_PLAYER ? 0 : 1];
            string controllersName = "";
            foreach (InputDevice device in activePlayerInput.GetComponent<PlayerInput>().devices)
            {
                string deviceName = device.name;
                if(deviceName.Contains("Mouse")) deviceName = deviceName.Replace("Mouse", LocalizationManager.Localize(_mouseKey));
                if(deviceName.Contains("Keyboard")) deviceName = deviceName.Replace("Keyboard", LocalizationManager.Localize(_keyboardKey));
                if(deviceName.Contains("Controller")) deviceName = deviceName.Replace("Controller", LocalizationManager.Localize(_controllerKey));
                if (deviceName.Contains("Gamepad")) deviceName = deviceName.Replace("Gamepad", LocalizationManager.Localize(_gamepadKey));

                controllersName += deviceName + ", ";
            }
            controllersName = controllersName.Remove(controllersName.Length - 2);

            _activeControllerText.text = LocalizationManager.Localize(_currentControllerKey) + " : " + controllersName;

#if UNITY_EDITOR
            if(DebugManager.Instance.ShowControllerKeyinRebinding)
                _activeControllerText.text = _activeControllerText.text + $"\n{SettingsSave.GetControllerKey(activePlayerInput.GetComponent<PlayerInput>().devices[0])}";
#endif

            // Switch input indication if two players
            UpdateControllerSwitchInfos(activePlayerInput.playerIndex);

            // Disable the right move (Gamepad : cannot rebind Move. Keyboard : can rebind it)
            _moveKeyboardInput.SetActive(PlayersInputManager.Instance.CurrentAllowedDevice is Keyboard || PlayersInputManager.Instance.CurrentAllowedDevice is Mouse);
            _moveGamepadInput.SetActive(PlayersInputManager.Instance.CurrentAllowedDevice is Gamepad);
        }

        private void UpdateControllerSwitchInfos(int playerIndex) => _switchInfos.SetActive(PlayersInputManager.Instance.PlayerConnectedCount() >= 2);
    }
}
