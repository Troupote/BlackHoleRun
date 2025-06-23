using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace BHR
{
    [CreateAssetMenu(fileName = "InputActionsName", menuName = "PlayerInput/InputActionsName")]
    public class InputActionsSO : ScriptableObject
    {
        [SerializeField, Required]
        private SerializedDictionary<string, Sprite> _bindingsControlPathToSprite; // do NOT rename or mess with it
        public Dictionary<string, Sprite> BindingsControlPathToSprite => _bindingsControlPathToSprite;

        [Header("Control schemes")]
        public string KeyboardScheme;
        public string GamepadScheme;

        [Header("Actions maps")]
        [Required] public string HumanoidActionMap;
        [Required] public string SingularityActionMap;
        [Required] public string UIActionMap;
        [Required] public string InactiveActionMap;
        [Required] public string EmptyActionMap;

        [Header("Humanoid/Singularity (Be sure that both actions have the same name and same bindings)")]
        [Required] public InputActionReference Move;
        [Required] public InputActionReference Look;
        [Required] public InputActionReference Jump;
        [Required] public InputActionReference Dash;
        //[Required] public InputActionReference Slide;
        [Required] public InputActionReference Restart;
        [Required] public InputActionReference Pause;

        [Header("Humanoid")]
        [Required] public InputActionReference Aim;
        [Required] public InputActionReference Throw;
        [Required] public InputActionReference Open;

        //[Header("Singularity")]
        //[Required] public InputActionReference Unmorph;

        [Header("Inactive")]
        [Required] public InputActionReference Reconnect;

        [Header("UI")]
        [Required] public InputActionReference Submit;
        [Required] public InputActionReference Cancel;
        [Required] public InputActionReference Switch;
        [Required] public InputActionReference Rebind;
    }
}
