using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace BHR
{
    public class HUDModuleUI : MonoBehaviour
    {
        [SerializeField, FoldoutGroup("Refs")] private TextMeshProUGUI _chronoText;
        [SerializeField, FoldoutGroup("Refs")] private GameObject _tutorielPopup;

        private void OnEnable()
        {
            GameManager.Instance.OnTimerChanged.AddListener(DisplayChrono);
        }

        private void Start()
        {
            TogglePopup(false);
            GameManager.Instance.OnLaunchLevel.AddListener((state) => TogglePopup(false));
        }

        private void DisplayChrono(float timeInSeconds)
        {
            _chronoText.text = UtilitiesFunctions.TimeFormat(timeInSeconds);
        }

        public void TogglePopup(bool enable) => _tutorielPopup.SetActive(enable);
    }
}
