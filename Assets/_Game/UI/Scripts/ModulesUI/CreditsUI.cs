using UnityEngine;

namespace BHR
{
    public class CreditsUI : AModuleUI
    {
        [SerializeField] private string _sceneName = "Credits";
        private void OnEnable()
        {
            ScenesManager.Instance.LoadSceneAdditive(_sceneName);
        }

        public override void Back()
        {
            ModuleManager.Instance.ClearNavigationHistoric();
            ModuleManager.Instance.Historic.Push(ModuleManager.Instance.GetModule(ModuleType.MAIN_TITLE));
            ScenesManager.Instance.UnloadSceneAdditive(_sceneName);
            base.Back();
        }
    }
}
