using UnityEngine;

namespace BHR
{
    public class UserSettingsModuleUI : AModuleUI
    {
        public override void Back()
        {
            PlayersInputManager.Instance.AllowOnlyOnePlayerUIInputs(false);
            base.Back();
        }
    }
}
