using UnityEngine;

namespace BHR
{
    public static class InputActions
    {
        public static string KeyboardScheme;
        public static string GamepadScheme;

        public static string HumanoidActionMap;
        public static string SingularityActionMap;
        public static string UIActionMap;
        public static string InactiveActionMap;
        public static string EmptyActionMap;

        public static string Move;
        public static string Look;
        public static string Jump;
        public static string Dash;
        public static string Slide;
        public static string Unmorph;
        public static string Aim;
        public static string Throw;
        public static string Pause;

        public static string Submit;
        public static string Cancel;
        public static string Switch;
        public static string Rebind;

        public static string[] AllActions;

        public static void Initialize(InputActionsSO inputActions)
        {
            KeyboardScheme = inputActions.KeyboardScheme;
            GamepadScheme = inputActions.GamepadScheme;

            HumanoidActionMap = inputActions.HumanoidActionMap;
            SingularityActionMap = inputActions.SingularityActionMap;
            UIActionMap = inputActions.UIActionMap;
            InactiveActionMap = inputActions.InactiveActionMap;
            EmptyActionMap = inputActions.EmptyActionMap;

            Move = inputActions.Move;
            Look = inputActions.Look;
            Jump = inputActions.Jump;
            Dash = inputActions.Dash;
            Slide = inputActions.Slide;
            Unmorph = inputActions.Unmorph;
            Aim = inputActions.Aim;
            Throw = inputActions.Throw;
            Pause = inputActions.Pause;

            Submit = inputActions.Submit;
            Cancel = inputActions.Cancel;
            Switch = inputActions.Switch;
            Rebind = inputActions.Rebind;

            AllActions = new string[]
            {
            Move, Look, Jump, Dash, Slide, Aim, Throw, Pause, Submit, Cancel
            };
        }
    }

}
