using UnityEngine;

namespace BHR
{
    public static class SettingsSave
    {
        #region Global settings
        private const string RESOLUTION_KEY = "Resolution"; private const string DEFAULT_RESOLUTION = "default"; public const char RESOLUTION_CAST = 'x';
        private const string IS_WINDOWED_KEY = "Windowed"; private const int DEFAULT_IS_WINDOWED = -1; // -1 : default detected | 0 : False | 1 : True
        #endregion

        #region Users settings
        private const string SENSITIVITY_KEY = "Sensitivity"; private const float DEFAULT_SENSITIVITY = 1f;
        private const string LEFT_STICK_DEADZONE_KEY = "LSDeadzone"; private const float DEFAULT_LEFT_STICK_DEADZONE = 0f;
        private const string RIGHT_STICK_DEADZONE_KEY = "RSDeadzone"; private const float DEFAULT_RIGHT_STICK_DEADZONE = 0.15f;
        #endregion

        #region Global settings
        public static void SaveResolution(string resolution = DEFAULT_RESOLUTION) => PlayerPrefs.SetString(RESOLUTION_KEY, resolution);
        public static string LoadResolution() => PlayerPrefs.GetString(RESOLUTION_KEY, DEFAULT_RESOLUTION);

        public static void SaveIsWindowed(int isWindowed = DEFAULT_IS_WINDOWED) => PlayerPrefs.SetInt(IS_WINDOWED_KEY, isWindowed);
        public static int LoadIsWindowed() => PlayerPrefs.GetInt(IS_WINDOWED_KEY, DEFAULT_IS_WINDOWED);

        #endregion

        #region User settings
        public static void SaveSensitivity(float value, int playerId) => PlayerPrefs.SetFloat(SENSITIVITY_KEY + playerId, value);
        public static float LoadSensitivity(int playerId) => PlayerPrefs.GetFloat(SENSITIVITY_KEY + playerId, DEFAULT_SENSITIVITY);

        public static void SaveLeftStickDeadzone(float value, int playerId) => PlayerPrefs.SetFloat(LEFT_STICK_DEADZONE_KEY + playerId, value);
        public static float LoadLeftStickDeadzone(int playerId) => PlayerPrefs.GetFloat(LEFT_STICK_DEADZONE_KEY + playerId, DEFAULT_LEFT_STICK_DEADZONE);
        public static void SaveRightStickDeadzone(float value, int playerId) => PlayerPrefs.SetFloat(RIGHT_STICK_DEADZONE_KEY + playerId, value);
        public static float LoadRightStickDeadzone(int playerId) => PlayerPrefs.GetFloat(RIGHT_STICK_DEADZONE_KEY + playerId, DEFAULT_RIGHT_STICK_DEADZONE);
        #endregion
    }
}
