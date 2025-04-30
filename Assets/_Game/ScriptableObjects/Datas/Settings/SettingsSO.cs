using Sirenix.OdinInspector;
using UnityEngine;

namespace BHR
{
    [CreateAssetMenu(fileName = "SettingsData", menuName = "Settings/Data")]
    public class SettingsSO : ScriptableObject
    {
        [Header("Global")]
        [SerializeField, FoldoutGroup("Camera")]
        private Vector2 mouseBaseSensitivity;
        public Vector2 MouseSensitivity => mouseBaseSensitivity;
        [SerializeField, FoldoutGroup("Camera")]
        private Vector2 gamepadBaseSensitivity;
        public Vector2 GamepadSensitivity => gamepadBaseSensitivity;

        //[Header("Users")]
    }
}
