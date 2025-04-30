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
        private const string LEFT_STICK_DEADZONE_KEY = "LSDeadzone"; private const float DEFAULT_LEFT_STICK_DEADZONE = 0f;
        private const string RIGHT_STICK_DEADZONE_KEY = "RSDeadzone"; private const float DEFAULT_RIGHT_STICK_DEADZONE = 0f;
        #endregion

        #region Save and load
        public static void SaveSensitivity(float value, int playerId) => PlayerPrefs.SetFloat(SENSITIVITY_KEY + playerId, value);
        public static float LoadSensitivity(int playerId) => PlayerPrefs.GetFloat(SENSITIVITY_KEY + playerId, DEFAULT_SENSITIVITY);

        public static void SaveLeftStickDeadzone(float value, int playerId) => PlayerPrefs.SetFloat(LEFT_STICK_DEADZONE_KEY + playerId, value);
        public static float LoadLeftStickDeadzone(int playerId) => PlayerPrefs.GetFloat(LEFT_STICK_DEADZONE_KEY + playerId, DEFAULT_LEFT_STICK_DEADZONE);
        public static void SaveRightStickDeadzone(float value, int playerId) => PlayerPrefs.SetFloat(RIGHT_STICK_DEADZONE_KEY + playerId, value);
        public static float LoadRightStickDeadzone(int playerId) => PlayerPrefs.GetFloat(RIGHT_STICK_DEADZONE_KEY + playerId, DEFAULT_RIGHT_STICK_DEADZONE);
        #endregion
    }
}
