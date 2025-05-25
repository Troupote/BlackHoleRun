using UnityEngine;
using UnityEngine.InputSystem;

namespace BHR.UILinkers
{
    public class RebindInputUILinker : AUserSettingsUILinker<string>
    {
        public override void SaveSetting(string value)
        {
            InputDevice controller = PlayersInputManager.Instance.CurrentAllowedDevice;
            SettingsSave.SaveBindings(controller, value);
        }

        protected override string LoadSetting()
        {
            InputDevice controller = PlayersInputManager.Instance.CurrentAllowedDevice;
            return SettingsSave.LoadBindings(controller);
        }

        protected override void UpdateUI()
        {
            
        }
    }
}
