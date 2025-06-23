using Assets.SimpleLocalization.Scripts;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace BHR.UILinkers
{
    public class HoldOrToggleAimUILinker : AUserSettingsUILinker<bool>
    {
        [SerializeField, Required] private TextMeshProUGUI _currentModeText;

        public override void SaveSetting(bool value)
        {
            InputDevice controller = PlayersInputManager.Instance.CurrentAllowedDevice;
            SettingsSave.SaveToggleAim(controller, value ? 1 : 0);
        }

        protected override bool LoadSetting()
        {
            InputDevice controller = PlayersInputManager.Instance.CurrentAllowedDevice;
            return _registered ? _savedValue : SettingsSave.LoadToggleAim(controller)==1;
        }

        protected override void UpdateUI()
        {
            GetComponent<Toggle>().isOn = LoadSetting();
            UpdateCurrentModeText(LoadSetting());
        }

        public void UpdateCurrentModeText(bool isToggle) => _currentModeText.text = isToggle ? LocalizationManager.Localize("Toggle") : LocalizationManager.Localize("Hold");
    }
}