using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.SimpleLocalization.Scripts
{
	/// <summary>
	/// Localize dropdown component.
	/// </summary>
    public class LocalizedDropdown : MonoBehaviour
    {
        public string[] LocalizationKeys;

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
            var dropdown = GetComponent<TMP_Dropdown>(); 

			for (var i = 0; i < LocalizationKeys.Length; i++)
	        {
		        dropdown.options[i].text = LocalizationManager.Localize(LocalizationKeys[i]);
	        }

	        if (dropdown.value < LocalizationKeys.Length)
	        {
		        dropdown.captionText.text = LocalizationManager.Localize(LocalizationKeys[dropdown.value]);
	        }
        }
    }
}