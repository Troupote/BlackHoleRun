using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BHR.UILinkers
{
    public abstract class ASettingsUILinker<T> : MonoBehaviour
    {
        [SerializeField] private bool _userSettings = false;
        [SerializeField, Tooltip("If true, will directly saved the detected value without passing by a Apply verification")]
        private bool _autoApply = true;
        protected T _savedValue;

        /// <summary>
        /// Save value (directly from UI detection if auto apply or from _savedValue)
        /// </summary>
        /// <param name="value">Value to save</param>
        public abstract void SaveSetting(T value);

        /// <summary>
        /// Load current saved settings to UI matching type
        /// </summary>
        /// <returns>Current saved value casted as UI matching type</returns>
        protected abstract T LoadSetting();

        /// <summary>
        /// Update the UI to match saved value
        /// </summary>
        protected abstract void UpdateUI();

        private void OnEnable()
        {
            // Update UI depending of saved data at enable
            UpdateUI();

            if (_userSettings) // Update UI when all user datas are loaded and when players switch between them in the menu
            {
                SettingsManager.Instance.OnUserDatasLoaded.AddListener(UpdateUI);
                PlayersInputManager.Instance.OnAllowedInputChanged.AddListener((AllowedPlayerInput) => UpdateUI());
            }
            else // Load UI when all global datas are loaded
                SettingsManager.Instance.OnGlobalDatasLoaded.AddListener(UpdateUI);

            // If not auto apply, saved data when "Apply" is triggered, and erase temp value when cancel
            if (!_autoApply)
            {
                SettingsManager.Instance.OnUserSettingsApplied.AddListener(ApplySetting);
                SettingsManager.Instance.OnUserSettingsCanceled.AddListener(CancelSetting);
            }
        }

        private void OnDisable()
        {
            if (_userSettings)
            {
                SettingsManager.Instance.OnUserDatasLoaded.RemoveListener(UpdateUI);
                PlayersInputManager.Instance.OnAllowedInputChanged.RemoveListener((AllowedPlayerInput) => UpdateUI());
            }
            else
                SettingsManager.Instance.OnGlobalDatasLoaded.RemoveListener(UpdateUI);

            if (!_autoApply)
            {
                SettingsManager.Instance.OnUserSettingsApplied.RemoveListener(ApplySetting);
                SettingsManager.Instance.OnUserSettingsCanceled.RemoveListener(CancelSetting);
            }
        }

        private void ApplySetting() => SaveSetting(_savedValue);
        private void CancelSetting() => _savedValue = LoadSetting();
    }
}
