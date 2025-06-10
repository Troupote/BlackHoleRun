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

        protected override int LoadSetting()
        {
            if(SettingsSave.LoadLanguage() == SettingsSave.START_LANGUAGE) // First time -> detected system language
                SettingsSave.SaveLanguage(Application.systemLanguage switch { SystemLanguage.English => "English", SystemLanguage.French => "Français", _ => SettingsSave.DEFAULT_LANGUAGE });


            TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();

            TMP_Dropdown.OptionData currentLanguage = dropdown.options.FirstOrDefault(o => o.text == SettingsSave.LoadLanguage());
            if (currentLanguage == null)
            {
                Debug.LogError(currentLanguage.text + " isn't supported.");

                SettingsSave.LoadLanguage(); // Reset language
                currentLanguage = dropdown.options.FirstOrDefault(o => o.text == SettingsSave.LoadLanguage());
            }
            return dropdown.options.IndexOf(currentLanguage);
        }

        protected override void UpdateUI() => GetComponent<TMP_Dropdown>().value = LoadSetting();
    }
}
