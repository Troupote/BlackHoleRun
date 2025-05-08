using BHR.UILinkers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BHR
{
    /// <summary>
    /// User settings is a variation of a global setting with a Apply verification (and not the same events)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AUserSettingsUILinker<T> : ASettingsUILinker<T>
    {
        // If not auto apply stuff
        [SerializeField, ReadOnly]
        protected T _savedValue;
        protected bool _registered = false;


        protected override void Start()
        {
            SettingsManager.Instance.OnUserDatasLoaded.AddListener(UpdateUI);
            SettingsManager.Instance.OnUserSettingsApplied.AddListener(ApplySetting);
            SettingsManager.Instance.OnUserSettingsCanceled.AddListener(CancelSetting);
            PlayersInputManager.Instance.OnAllowedInputChanged.AddListener((AllowedPlayerInput) => UpdateUI());
        }

        /// <summary>
        /// Register the UI detected new value
        /// </summary>
        /// <param name="value">UI detected value</param>
        public void RegisterSetting(T value)
        {
            _savedValue = value;
            _registered = true;
        }

        /// <summary>
        /// Apply by saving the registered value 
        /// </summary>
        private void ApplySetting()
        {
            if(_registered)
            {
                SaveSetting(_savedValue);
                _registered = false;
            }
        }

        /// <summary>
        /// Cancel by reseting registered value to the last saved value (and cancel the register)
        /// </summary>
        private void CancelSetting()
        {
            _savedValue = LoadSetting();
            _registered = false;
        }
    }
}
