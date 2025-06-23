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
        //public static string Slide;
        //public static string Unmorph;
        public static string Aim;
        public static string Throw;
        public static string Restart;
        public static string Pause;
        public static string Reconnect;
        public static string Open;

        public static string Submit;
        public static string Cancel;
        public static string Switch;
        public static string Rebind;

        public static void Initialize(InputActionsSO inputActions)
        {
            KeyboardScheme = inputActions.KeyboardScheme;
            GamepadScheme = inputActions.GamepadScheme;

            HumanoidActionMap = inputActions.HumanoidActionMap;
            SingularityActionMap = inputActions.SingularityActionMap;
            UIActionMap = inputActions.UIActionMap;
            InactiveActionMap = inputActions.InactiveActionMap;
            EmptyActionMap = inputActions.EmptyActionMap;

            Move = inputActions.Move.action.name;
            Look = inputActions.Look.action.name;
            Jump = inputActions.Jump.action.name;
            Dash = inputActions.Dash.action.name;
            //Slide = inputActions.Slide.action.name;
            //Unmorph = inputActions.Unmorph.action.name;
            Aim = inputActions.Aim.action.name;
            Throw = inputActions.Throw.action.name;
            Restart = inputActions.Restart.action.name;
            Pause = inputActions.Pause.action.name;
            Reconnect = inputActions.Reconnect.action.name;
            Open = inputActions.Open.action.name;

            Submit = inputActions.Submit.action.name;
            Cancel = inputActions.Cancel.action.name;
            Switch = inputActions.Switch.action.name;
            Rebind = inputActions.Rebind.action.name;
        }
    }

}
