using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputActionsName", menuName = "PlayerInput/InputActionsName")]
public class InputActionsSO : ScriptableObject
{
    [Header("Control schemes")]
    public string KeyboardScheme;
    public string GamepadScheme;

    [Header("Actions maps")]
    public string HumanoidActionMap;
    public string SingularityActionMap;
    public string UIActionMap;
    public string InactiveActionMap;
    public string EmptyActionMap;

    [Header("Humanoid/Singularity")]
    public string Move;
    public string Look;
    public string Jump;
    public string Dash;
    public string Slide;
    public string Unmorph;
    public string Aim;
    public string Throw;
    public string Pause;

    [Header("UI")]
    public string Submit;
    public string Cancel;
    public string Switch;
    public string Rebind;

    public string[] AllActions => new string[] {Move, Look, Jump, Dash, Slide, Aim, Throw, Pause, Submit, Cancel};
}
