using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace BHR.UILinkers
{
    public class InvertAxeUILinker : AUserSettingsUILinker<bool>
    {
        //[SerializeField]
        //private Axes _axe = Axes.Y;

        public override void SaveSetting(bool value)
        {
            InputDevice controller = PlayersInputManager.Instance.CurrentAllowedDevice;
            SettingsSave.SaveInvertAxeY(controller, value ? 1 : 0);
        }

        protected override bool LoadSetting()
        {
            InputDevice controller = PlayersInputManager.Instance.CurrentAllowedDevice;
            return _registered ? _savedValue : SettingsSave.LoadInvertAxeY(controller);
        }

        protected override void UpdateUI() => GetComponent<Toggle>().isOn = LoadSetting();
    }
}
