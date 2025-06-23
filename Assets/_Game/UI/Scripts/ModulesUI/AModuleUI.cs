using UnityEngine;

namespace BHR
{
    public abstract class AModuleUI : MonoBehaviour
    {
        public virtual void Back()
        {
            ModuleManager.Instance.Back();
        }
    }
}
