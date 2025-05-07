using Sirenix.OdinInspector;
using UnityEngine;

namespace BHR.UILinkers
{
    public abstract class ASettingsUILinker<T> : MonoBehaviour
    {
        /// <summary>
        /// Save value (directly from UI detection if auto apply or from _savedValue)
        /// </summary>
        /// <param name="value">Value to save</param>
        public abstract void SaveSetting(T value);

        /// <summary>
        /// Load current saved settings to UI matching type
        /// </summary>
        /// <returns>Current saved value casted as UI matching type</returns>
        protected abstract T LoadSetting();

        /// <summary>
        /// Update the UI to match saved value
        /// </summary>
        protected abstract void UpdateUI();

        protected virtual void Start() => SettingsManager.Instance.OnGlobalDatasLoaded.AddListener(UpdateUI);

        private void OnEnable() => UpdateUI();
    }
}
