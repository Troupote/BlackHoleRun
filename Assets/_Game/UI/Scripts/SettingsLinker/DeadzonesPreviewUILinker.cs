using UnityEngine;
using UnityEngine.UI;


namespace BHR.UILinkers
{
    public class DeadzonesPreviewUILinker : ASettingsUILinker<bool>
    {
        public override void SaveSetting(bool value)
        {
            SettingsSave.SaveDeadzonePreview(value ? 1 : 0);
            SettingsManager.Instance.UpdateDeadzonePreview();
        }

        protected override bool LoadSetting() => SettingsSave.LoadDeadzonePreview() switch { 1 => true, 0 => false, _ => true };

        protected override void UpdateUI()
        {
            GetComponent<Toggle>().isOn = LoadSetting();
        }
    }
}
