using UnityEngine;
using UnityEngine.InputSystem;

namespace BHR
{
    public static class SettingsSave
    {
        #region Global settings keys and default values
        private const string RESOLUTION_KEY = "Resolution"; private const string DEFAULT_RESOLUTION = "default"; public const char RESOLUTION_CAST = 'x';
        private const string IS_WINDOWED_KEY = "Windowed"; private const int DEFAULT_IS_WINDOWED = -1; // -1 : default detected | 0 : False | 1 : True
        private const string LANGUAGE_KEY = "Language"; private const string DEFAULT_LANGUAGE = "English";
        private const string MASTER_VOLUME_KEY = "MasterVolume"; private const float DEFAULT_MASTER_VOLUME = 0.5f;
        private const string MUSIC_VOLUME_KEY = "MusicVolume"; private const float DEFAULT_MUSIC_VOLUME = 0.5f;
        private const string MASTER_SOUND_KEY = "MasterVolume"; private const float DEFAULT_SOUND_VOLUME = 0.5f;
        #endregion

        #region Users settings keys and default values
        private const string SENSITIVITY_X_KEY = "SensitivityX/"; private const float DEFAULT_SENSITIVITY_X = 1f;
        private const string SENSITIVITY_Y_KEY = "SensitivityY/"; private const float DEFAULT_SENSITIVITY_Y = 1f;
        private const string INVERT_AXE_Y_KEY = "InvertAxeY/"; private const int DEFAULT_INVERT_AXE_Y = 0;
        private const string LEFT_STICK_DEADZONE_KEY = "LSDeadzone/"; private const float DEFAULT_LEFT_STICK_DEADZONE = 0f;
        private const string RIGHT_STICK_DEADZONE_KEY = "RSDeadzone/"; private const float DEFAULT_RIGHT_STICK_DEADZONE = 0.15f;
        #endregion

        #region Global settings save and load
        public static void SaveResolution(string resolution = DEFAULT_RESOLUTION) => PlayerPrefs.SetString(RESOLUTION_KEY, resolution);
        public static string LoadResolution() => PlayerPrefs.GetString(RESOLUTION_KEY, DEFAULT_RESOLUTION);

        public static void SaveIsWindowed(int isWindowed = DEFAULT_IS_WINDOWED) => PlayerPrefs.SetInt(IS_WINDOWED_KEY, isWindowed);
        public static int LoadIsWindowed() => PlayerPrefs.GetInt(IS_WINDOWED_KEY, DEFAULT_IS_WINDOWED);

        public static void SaveLanguage(string language = DEFAULT_LANGUAGE) => PlayerPrefs.SetString(LANGUAGE_KEY, language);
        public static string LoadLanguage() => PlayerPrefs.GetString(LANGUAGE_KEY, DEFAULT_LANGUAGE);

        public static void SaveMasterVolume(float value = DEFAULT_MASTER_VOLUME) => PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
        public static float LoadMasterVolume() => PlayerPrefs.GetFloat(MASTER_SOUND_KEY, DEFAULT_MASTER_VOLUME);
        public static void SaveMusicVolume(float value = DEFAULT_MUSIC_VOLUME) => PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        public static float LoadMusicVolume() => PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_MUSIC_VOLUME);
        public static void SaveSoundsVolume(float value = DEFAULT_SOUND_VOLUME) => PlayerPrefs.GetFloat(MASTER_SOUND_KEY, DEFAULT_SOUND_VOLUME);
        public static float LoadSoundsVolume() => PlayerPrefs.GetFloat(MASTER_SOUND_KEY, DEFAULT_SOUND_VOLUME);

        #endregion

        #region User settings save and load
        public static string GetControllerKey(InputDevice controller)
        {
            string controllerKey = "Not-referenced";
            if (controller is Mouse || controller is Keyboard)
                controllerKey = "KeyboardMouse";
            else if (controller is Gamepad)
                controllerKey = $"{controller.device.description.manufacturer}_{controller.device.description.product}_{controller.device.description.serial}";
            return controllerKey;
        }

        public static void SaveSensitivityX(InputDevice controller, float value = DEFAULT_SENSITIVITY_X) => PlayerPrefs.SetFloat(SENSITIVITY_X_KEY + GetControllerKey(controller), value);
        public static float LoadSensitivityX(InputDevice controller) => PlayerPrefs.GetFloat(SENSITIVITY_X_KEY + GetControllerKey(controller), DEFAULT_SENSITIVITY_X);
        public static void SaveSensitivityY(InputDevice controller, float value = DEFAULT_SENSITIVITY_Y) => PlayerPrefs.SetFloat(SENSITIVITY_Y_KEY + GetControllerKey(controller), value);
        public static float LoadSensitivityY(InputDevice controller) => PlayerPrefs.GetFloat(SENSITIVITY_Y_KEY + GetControllerKey(controller), DEFAULT_SENSITIVITY_Y);

        public static void SaveInvertAxeY(InputDevice controller, int value = DEFAULT_INVERT_AXE_Y) => PlayerPrefs.SetInt(INVERT_AXE_Y_KEY + GetControllerKey(controller), value);
        public static bool LoadInvertAxeY(InputDevice controller) => PlayerPrefs.GetInt(INVERT_AXE_Y_KEY + GetControllerKey(controller), DEFAULT_INVERT_AXE_Y) == 1;

        public static void SaveLeftStickDeadzone(InputDevice controller, float value = DEFAULT_LEFT_STICK_DEADZONE) => PlayerPrefs.SetFloat(LEFT_STICK_DEADZONE_KEY + GetControllerKey(controller), value);
        public static float LoadLeftStickDeadzone(InputDevice controller) => PlayerPrefs.GetFloat(LEFT_STICK_DEADZONE_KEY + GetControllerKey(controller), DEFAULT_LEFT_STICK_DEADZONE);
        public static void SaveRightStickDeadzone(InputDevice controller, float value = DEFAULT_RIGHT_STICK_DEADZONE) => PlayerPrefs.SetFloat(RIGHT_STICK_DEADZONE_KEY + GetControllerKey(controller), value);
        public static float LoadRightStickDeadzone(InputDevice controller) => PlayerPrefs.GetFloat(RIGHT_STICK_DEADZONE_KEY + GetControllerKey(controller), DEFAULT_RIGHT_STICK_DEADZONE);
        #endregion
    }
}
