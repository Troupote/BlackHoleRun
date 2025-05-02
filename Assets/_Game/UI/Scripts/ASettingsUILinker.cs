using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BHR.UILinkers
{
    public abstract class ASettingsUILinker<T> : MonoBehaviour
    {
        public abstract void SaveSetting(T value);
        protected abstract void LoadSetting();

        private void OnEnable()
        {
            LoadSetting();
            SettingsManager.Instance.OnAllDataLoaded.AddListener(LoadSetting);
        }

        private void OnDisable()
        {
            SettingsManager.Instance.OnAllDataLoaded.RemoveListener(LoadSetting);
        }
    }
}
