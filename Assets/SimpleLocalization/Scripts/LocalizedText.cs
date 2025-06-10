using BHR;
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
        protected enum TextCase { NONE, UPPERCASE, LOWERCASE, NOUN}
        [SerializeField, Tooltip("None will apply nothing on the localized text. Noun put the first letter in capital and the left in lowercase")] 
        private TextCase textCase = TextCase.NONE;

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
            string locText = LocalizationManager.Localize(LocalizationKey);
            string outputText = textCase switch { TextCase.UPPERCASE => locText.ToUpper(), TextCase.LOWERCASE => locText.ToLower(), TextCase.NOUN => UtilitiesFunctions.ToLowerWithFirstUpper(locText), _  => locText };
            if (TryGetComponent<Text>(out Text text))
            {
                text.text = outputText;
                LayoutRebuilder.ForceRebuildLayoutImmediate(text.rectTransform);
            }
            if (TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI textMeshPro))
            {
                textMeshPro.text = outputText;
                LayoutRebuilder.ForceRebuildLayoutImmediate(textMeshPro.rectTransform);
            }
        }
    }
}