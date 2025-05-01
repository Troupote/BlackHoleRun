using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BHR.UILinkers
{
    public class LanguageUILinker : ASettingsUILinker<int>
    {
        public override void SaveSetting(int choice)
        {
            SettingsSave.SaveLanguage(GetComponent<TMP_Dropdown>().options[choice].text);
            SettingsManager.Instance.UpdateLanguage();
        }

        protected override void LoadSetting()
        {
            TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();

            TMP_Dropdown.OptionData currentLanguage = dropdown.options.FirstOrDefault(o => o.text == SettingsSave.LoadLanguage());
            if(currentLanguage == null)
            {
                Debug.LogError(currentLanguage.text + " isn't supported.");

                SettingsSave.LoadLanguage();
                currentLanguage = dropdown.options.FirstOrDefault(o => o.text == SettingsSave.LoadLanguage());
            }

            dropdown.value = dropdown.options.IndexOf(currentLanguage);
        }
    }
}
