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

        [Header("Debug Keyboard Shortcuts"), Tooltip("Set true to enable keyboard debug shortcuts"), SerializeField]
        private bool _enableDebugKeyboardShortcuts;
        public bool DebugKeyboardShortcutsEnabled => _enableDebugKeyboardShortcuts && _debug;

        [Header("HUD"), Tooltip("Enable/Disable start animation"), ShowIf(nameof(_debug)), SerializeField]
        private bool _disableStartAnimation;

        public bool DisableStartAnimation => _disableStartAnimation && _debug;

        [Header("InGame"), Tooltip("Enable/disable phase/checkpoint data text"), ShowIf(nameof(_debug)), SerializeField]
        private bool _enableCheckpointInfosText;
        [SerializeField, Required, ShowIf(nameof(_debug))]
        private TextMeshProUGUI _checkpointHUDInfosText; 
        public TextMeshProUGUI CheckpointHUDInfosText => _checkpointHUDInfosText;
        public bool CheckpointInfosTextEnabled => _enableCheckpointInfosText && _debug;

        private void Start()
        {
            transform.GetChild(0).gameObject.SetActive(_debug);
            ModuleManager.Instance.OnModuleEnabled.AddListener(OnModuleEnabled);
        }

        private void OnModuleEnabled(GameObject module, bool withBack)
        {
            if (CheckpointInfosTextEnabled && CheckpointHUDInfosText != null)
                CheckpointHUDInfosText.gameObject.SetActive(ModuleManager.Instance.ModulesRef[module] == ModuleType.HUD);
           
        }
    }
}
#endif