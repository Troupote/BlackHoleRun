using UnityEngine;
using UnityEngine.InputSystem;

namespace BHR
{
    public class PlayerInputReceiver : ManagerSingleton<PlayerInputReceiver>
    {
        public override void Awake()
        {
            SetInstance(false);
        }
        public void HandleInput(InputAction.CallbackContext ctx)
        {
            // Specific global actions (no need to diferentiate players)
            if (ctx.performed && ctx.action.name == InputActions.Pause)
                GameManager.Instance.TogglePause();
        }
    }
}
