using UnityEngine;

namespace BHR
{
    public class SettingsModuleUI : AModuleUI
    {
        public void ResetSettings()
        {
            SettingsManager.Instance.ResetGlobalSettings();
        }
    }
}
