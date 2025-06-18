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
        private TextCase _textCase = TextCase.NONE;
        [SerializeField, Tooltip("Add a fixed text after the localized one")]
        private string _addedText;
        [SerializeField, HideIf(nameof(_addedText), null)]
        private bool _addedTextBeforeLocalizedText = false;

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
            string entireText = (_addedTextBeforeLocalizedText ? _addedText : "") + locText + (!_addedTextBeforeLocalizedText ? _addedText : "");
            string outputText = _textCase switch { TextCase.UPPERCASE => entireText.ToUpper(), TextCase.LOWERCASE => entireText.ToLower(), TextCase.NOUN => UtilitiesFunctions.ToLowerWithFirstUpper(entireText), _  => entireText };
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