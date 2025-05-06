using UnityEngine;
using UnityEngine.UI;


namespace BHR.UILinkers
{
    public class IsWindowedUILinker : ASettingsUILinker<bool>
    {
        public override void SaveSetting(bool value)
        {
            SettingsSave.SaveIsWindowed(value ? 1 : 0);
            SettingsManager.Instance.UpdateResolutionAndWindowed();
        }

        protected override bool LoadSetting() => SettingsSave.LoadIsWindowed() switch { 1 => true, 0 => false, _ => !Screen.fullScreen };

        protected override void UpdateUI()
        {
            GetComponent<Toggle>().isOn = LoadSetting();
        }
    }
}
