using Assets.SimpleLocalization.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BHR
{
    public class SettingsManager : ManagerSingleton<SettingsManager>
    {
        [Required]
        public SettingsSO BaseSettings;

        public UnityEvent OnGlobalDatasLoaded;
        public UnityEvent OnUserDatasLoaded;
        public UnityEvent<string> OnUserBindingsLoaded;

        public UnityEvent OnUserSettingsApplied;
        public UnityEvent OnUserSettingsCanceled;

        private void Start()
        {
            LoadAllSavedData();
        }

        #region Global settings

        public void UpdateResolutionAndWindowed()
        {
            // Resolution
            string[] resolution = SettingsSave.LoadResolution().Split(SettingsSave.RESOLUTION_CAST);
            bool useDefault = resolution.Length != 2;

            int width = useDefault ? Screen.width : int.Parse(resolution[0]);
            int height = useDefault ? Screen.height : int.Parse(resolution[1]);

            // Windowed or not
            FullScreenMode mode = SettingsSave.LoadIsWindowed() switch { -1 => Screen.fullScreenMode, 0 => FullScreenMode.ExclusiveFullScreen, 1 => FullScreenMode.Windowed, _ => Screen.fullScreenMode };

            Screen.SetResolution(width, height, mode);
        }

        public void UpdateLanguage()
        {
            LocalizationManager.Language = SettingsSave.LoadLanguage();
        }

        public void UpdateVolume()
        {
            if (AudioManager.Instance == null) return;
            AudioManager.Instance.ApplyVolumesToAllEvents();
        }

        public void ApplyUserSettings() => OnUserSettingsApplied?.Invoke();
        public void CancelUserSettings() => OnUserSettingsCanceled?.Invoke();

        public void ResetGlobalSettings()
        {
            SettingsSave.SaveResolution();
            SettingsSave.SaveIsWindowed();
            SettingsSave.SaveLanguage();
            SettingsSave.SaveMasterVolume();
            SettingsSave.SaveMusicVolume();
            SettingsSave.SaveSoundsVolume();

            LoadAllSavedData();
        }

        public void ResetAdvancedUserSettings(InputDevice controller)
        {
            SettingsSave.SaveSensitivityX(controller);
            SettingsSave.SaveSensitivityY(controller);
            SettingsSave.SaveInvertAxeY(controller);
            //SettingsSave.SaveLeftStickDeadzone(controller);
            //SettingsSave.SaveRightStickDeadzone(controller);
            SettingsSave.SaveToggleAim(controller);

            LoadUserData(controller);
        }

        public void ResetBindingsSettings(InputDevice controller)
        {
            SettingsSave.SaveBindings(controller);
            OnUserBindingsLoaded?.Invoke(SettingsSave.LoadBindings(controller));
        }

        private void LoadAllSavedData()
        {
            UpdateLanguage();
            UpdateResolutionAndWindowed();
            UpdateVolume();
            OnGlobalDatasLoaded?.Invoke();
        }

        private void LoadUserData(InputDevice controller)
        {
            OnUserDatasLoaded?.Invoke();
        }


        #endregion
    }
}
