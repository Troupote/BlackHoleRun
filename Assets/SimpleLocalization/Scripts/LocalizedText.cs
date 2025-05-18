using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.SimpleLocalization.Scripts
{
	/// <summary>
	/// Localize text component.
	/// </summary>
    public class LocalizedText : MonoBehaviour
    {
        public string LocalizationKey;

        public void Start()
        {
            Localize();
            LocalizationManager.OnLocalizationChanged += Localize;
        }

        public void OnDestroy()
        {
            LocalizationManager.OnLocalizationChanged -= Localize;
        }

        private void Localize()
        {
            if(TryGetComponent<Text>(out Text text))
                text.text = LocalizationManager.Localize(LocalizationKey);
            if (TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI textMeshPro))
                textMeshPro.text = LocalizationManager.Localize(LocalizationKey);
        }
    }
}