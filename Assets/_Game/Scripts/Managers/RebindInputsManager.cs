using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BHR
{
    public class RebindInputsManager : ManagerSingleton<RebindInputsManager>
    {
        public List<string> RegisterednewPath = new List<string>();

        private void Start()
        {
            PlayersInputManager.Instance.OnUIInput.AddListener(HandleInput);
            SettingsManager.Instance.OnUserSettingsApplied.AddListener(ClearRegisteredPath);
            SettingsManager.Instance.OnUserSettingsCanceled.AddListener(ClearRegisteredPath);
        }

        private void ClearRegisteredPath() => RegisterednewPath.Clear();

        private void HandleInput(InputAction.CallbackContext ctx)
        {
            if(ctx.performed && ctx.action.name == InputActions.Switch && ModuleManager.Instance.CurrentModule == ModuleManager.Instance.GetModule(ModuleType.MAP_REBINDING))
            {
                TogglePlayerRebinding();
            }
        }

        private void TogglePlayerRebinding()
        {
            SettingsManager.Instance.ApplyUserSettings();
            PlayersInputManager.Instance.ToggleCurrentAllowedInput();
        }

        #region Rebinding
        public bool IsRebinding { get; private set; }

        public void ToggleRebinding(float time) => Invoke("ToggleRebinding",time);
        public void ToggleRebinding() => IsRebinding = !IsRebinding;
        #endregion
    }
}
