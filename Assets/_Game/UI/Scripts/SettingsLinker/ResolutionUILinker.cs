using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BHR.UILinkers
{
    public class ResolutionUILinker : ASettingsUILinker<int>
    {
        public override void SaveSetting(int choice)
        {
            SettingsSave.SaveResolution(GetComponent<TMP_Dropdown>().options[choice].text);
            SettingsManager.Instance.UpdateResolutionAndWindowed();
        }

        protected override void LoadSetting()
        {
            TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();

            TMP_Dropdown.OptionData currentResolution = dropdown.options.FirstOrDefault(o => o.text == SettingsSave.LoadResolution());
            if(currentResolution == null)
            {
                // Default detected resolution
                string resolutionText = $"{Screen.width} {SettingsSave.RESOLUTION_CAST} {Screen.height}";
                currentResolution = dropdown.options.FirstOrDefault(o => o.text == resolutionText);

                if(currentResolution == null) // if detected resolution isn't in the game resolutions
                {
                    // @todo réfléchir à quoi faire
                    Debug.LogWarning($"{resolutionText} ne fait pas partie des résolutions proposées par BlackHoleRun");
                }
            }

            dropdown.value = dropdown.options.IndexOf(currentResolution);
        }
    }
}
