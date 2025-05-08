using UnityEngine;

namespace BHR
{
    public class SettingsModuleUI : AModuleUI
    {
        public void ResetSettings() => SettingsManager.Instance.ResetGlobalSettings();

        public void OnAccessUserSettings()
        {
            PlayersInputManager.Instance.AllowOnlyOnePlayerUIInputs(true);
        }
    }
}
