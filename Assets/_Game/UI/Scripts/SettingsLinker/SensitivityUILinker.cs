using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BHR.UILinkers
{
    public class SensitivityUILinker : ASettingsUILinker<float>
    {
        [SerializeField]
        private Axes _axe;

        public void RegisterSettings(float value) => _savedValue = value;

        public override void SaveSetting(float value)
        {
            InputDevice controller = PlayersInputManager.Instance.CurrentAllowedDevice;
            switch (_axe)
            {
                case Axes.X: SettingsSave.SaveSensitivityX(controller, value); break;
                case Axes.Y: SettingsSave.SaveSensitivityY(controller, value); break;
                default: Debug.LogError($"Please choose a correct axe for {gameObject.name}"); break;
            }
        }

        protected override float LoadSetting()
        {
            InputDevice controller = PlayersInputManager.Instance.CurrentAllowedDevice;
            return _axe switch
            {
                Axes.X => SettingsSave.LoadSensitivityX(controller),
                Axes.Y => SettingsSave.LoadSensitivityY(controller),
                _ => -1f
            };
        }

        protected override void UpdateUI()
        {
            if(LoadSetting() == -1f)
            {
                Debug.LogError("Please enter a correct axe (X or Y) for {gameObject.name}");
                return;
            }
            GetComponent<Slider>().SetValueWithoutNotify(LoadSetting());
        }
    }
}
