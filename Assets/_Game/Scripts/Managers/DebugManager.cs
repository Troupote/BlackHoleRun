#if UNITY_EDITOR
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace BHR
{
    public class DebugManager : ManagerSingleton<DebugManager>
    {
        [SerializeField, Tooltip("Enable DebugManager")]
        private bool _debug;

        [Header("Debug Keyboard Shortcuts"), Tooltip("Set true to enable keyboard debug shortcuts"), SerializeField, ShowIf(nameof(_debug))]
        private bool _enableDebugKeyboardShortcuts;
        public bool DebugKeyboardShortcutsEnabled => _enableDebugKeyboardShortcuts && _debug;

        [Header("Unlocker"), SerializeField]
        private bool _unlockedAllLevels;
        public bool UnlockedAllLevels => _unlockedAllLevels && _debug;

        [Header("HUD"), Tooltip("Enable/Disable start animation"), ShowIf(nameof(_debug)), SerializeField]
        private bool _disableStartAnimation;
        public bool DisableStartAnimation => _disableStartAnimation && _debug;

        [Header("InGame")]
        [SerializeField]
        private bool _forceTutoriel;
        public bool ForceTutoriel => _forceTutoriel && _debug;
        [SerializeField, ShowIf(nameof(_debug))]
        private bool _disableTutorielPopup = false;
        public bool DisableTutorielPopup => _disableTutorielPopup && _debug;
        [SerializeField, ShowIf(nameof(_debug))]
        private bool _immortal;
        public bool Immortal => _immortal && _debug;
        [SerializeField, ShowIf(nameof(_debug))]
        private bool _singularityInstantComeback;
        public bool SingularityInstantComeback => _singularityInstantComeback && _debug;


        [Header("Rebinding"), SerializeField, ShowIf(nameof(_debug))]
        private bool _showControllerKeyinRebinding;
        public bool ShowControllerKeyinRebinding => _showControllerKeyinRebinding && _debug;

        private void Start()
        {
            //transform.GetChild(0).gameObject.SetActive(_debug);
        }
    }
}
#endif