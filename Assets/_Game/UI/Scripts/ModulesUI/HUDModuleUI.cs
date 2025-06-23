using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace BHR
{
    public class HUDModuleUI : MonoBehaviour
    {
        [SerializeField, FoldoutGroup("Refs")] private TextMeshProUGUI _chronoText;
        [SerializeField, FoldoutGroup("Refs")] private GameObject _tutorielPopup;
        [SerializeField, FoldoutGroup("Refs")] private GameObject _crosshair;
        [SerializeField, FoldoutGroup("Settings")] private Color _startChronoColor;
        [SerializeField, FoldoutGroup("Settings")] private Color _endChronoColor;

        private void OnEnable()
        {
            GameManager.Instance.OnTimerChanged.AddListener(DisplayChrono);
        }

        private void Start()
        {
            TogglePopup(false);
            GameManager.Instance.OnLaunchLevel.AddListener((state) => TogglePopup(false));
            _chronoText.gameObject.SetActive(false);

            GameManager.Instance.OnStartLevel.AddListener(() =>  _chronoText.gameObject.SetActive(true));
        }



        private void DisplayChrono(float timeInSeconds, bool practiceMode)
        {
            if(timeInSeconds <= -1f)
            {
                _chronoText.gameObject.SetActive(false);
                return;
            }
            float t = practiceMode ? 1 - (timeInSeconds / GameManager.Instance.SelectedLevel.BestTime()) : timeInSeconds / GameManager.Instance.SelectedLevel.Times[MedalsType.EARTH];
            _chronoText.color = Color.Lerp(_endChronoColor, _startChronoColor, t);
            _chronoText.text = UtilitiesFunctions.TimeFormat(timeInSeconds);
        }

        public void TogglePopup(bool enable) => _tutorielPopup.SetActive(enable);
        public void ToggleCrosshair(bool enable) => _crosshair.SetActive(enable);
    }
}
