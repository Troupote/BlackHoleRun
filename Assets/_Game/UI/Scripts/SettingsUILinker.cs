using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BHR.UILinkers
{
    public abstract class SettingsUILinker<T> : MonoBehaviour
    {
        public abstract void SaveSetting(T value);
        protected abstract void LoadSetting();

        private void OnEnable()
        {
            LoadSetting();
        }
    }
}
