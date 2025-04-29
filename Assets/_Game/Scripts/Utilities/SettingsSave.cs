using UnityEngine;

namespace BHR
{
    public static class SettingsSave
    {
        #region Global settings
        private const string MASTER_VOLUME_KEY = "";
        #endregion

        #region Users settings
        private const string SENSITIVITY_KEY = "Sensitivity"; private const float DEFAULT_SENSITIVITY = 1f;
        #endregion

        #region Save and load
        public static void SaveSensitivity(float value, int playerId) => PlayerPrefs.SetFloat(SENSITIVITY_KEY + playerId, value);
        public static float LoadSensitivity(int playerId) => PlayerPrefs.GetFloat(SENSITIVITY_KEY + playerId, DEFAULT_SENSITIVITY);
        #endregion
    }
}
