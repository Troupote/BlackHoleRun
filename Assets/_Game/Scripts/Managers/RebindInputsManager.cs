using UnityEngine;
using UnityEngine.InputSystem;

namespace BHR
{
    public class RebindInputsManager : MonoBehaviour
    {

        private void OnEnable()
        {
            PlayersInputManager.Instance.OnUIInput.AddListener(HandleInput);
        }

        private void OnDisable()
        {
            PlayersInputManager.Instance.OnUIInput.RemoveListener(HandleInput);
        }

        private void HandleInput(InputAction.CallbackContext ctx)
        {
            if(ctx.performed && ctx.action.name == InputActions.Switch && ModuleManager.Instance.CurrentModule == ModuleManager.Instance.GetModule(ModuleManager.ModuleType.MAP_REBINDING))
            {
                TogglePlayerRebinding();
            }
        }

        private void TogglePlayerRebinding()
        {
            // @todo save/cancel popup ? or auto choice (save?)
            PlayersInputManager.Instance.ToggleCurrentAllowedInput();
        }
    }
}
