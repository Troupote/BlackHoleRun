using Sirenix.OdinInspector;
using UnityEngine;

namespace BHR
{
    public class DebugManager : ManagerSingleton<DebugManager>
    {
        [SerializeField, Tooltip("Enable DebugManager")]
        private bool _debug;

        [Header("HUD"), Tooltip("Enable/Disable start animation"), ShowIf(nameof(_debug)), SerializeField]
        private bool _disableStartAnimation;

        public bool DisableStartAnimation => _disableStartAnimation && _debug;
    }
}
