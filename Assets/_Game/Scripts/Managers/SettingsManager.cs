using UnityEngine;
using UnityEngine.Events;

namespace BHR
{
    public class SettingsManager : ManagerSingleton<SettingsManager>
    {
        public UnityEvent OnAllDataLoaded;

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
            Debug.Log(SettingsSave.LoadLanguage() + " is active");
            // @todo Language update (not now)
        }

        public void UpdateVolume()
        {
            // @todo Thybault c'est ici que tu dois cook
        }

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

        private void LoadAllSavedData()
        {
            UpdateLanguage();
            UpdateResolutionAndWindowed();
            UpdateVolume();
            OnAllDataLoaded?.Invoke();
        }
        #endregion
    }
}
