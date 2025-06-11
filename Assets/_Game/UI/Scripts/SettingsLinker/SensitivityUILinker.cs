using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BHR.UILinkers
{
    public class SensitivityUILinker : AUserSettingsUILinker<float>
    {
        [SerializeField]
        private Axes _axe;

        public override void SaveSetting(float value)
        {
            InputDevice controller = PlayersInputManager.Instance.CurrentAllowedDevice;
            switch (_axe)
            {
                case Axes.X: SettingsSave.SaveSensitivityX(controller, value); break;
                case Axes.Y: SettingsSave.SaveSensitivityY(controller, value); break;
                default: Debug.LogError($"Please choose a correct axe for {gameObject.name}"); break;
            }
            _registered = false;
        }

        protected override float LoadSetting()
        {
            if( _registered ) return _savedValue;
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
            float value = _registered ? _savedValue : LoadSetting();
            if(value == -1f)
            {
                Debug.LogError("Please enter a correct axe (X or Y) for {gameObject.name}");
                return;
            }
            GetComponent<Slider>().SetValueWithoutNotify(value);
        }
    }
}
