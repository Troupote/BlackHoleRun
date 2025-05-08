using UnityEngine;

namespace BHR
{
    // DISCONNECTED : Player controller's not detected by Unity
    // KEYBOARD / GAMEPAD : Player is connected with the specified type controller
    public enum PlayerControllerState {DISCONNECTED = 0, KEYBOARD = 1, GAMEPAD = 2 };
    public enum PlayerReadyState {NONE = 0, LOGGED_OUT = 1, CONNECTED = 2, READY = 3}
}
