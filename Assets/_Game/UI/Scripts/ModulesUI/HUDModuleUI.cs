using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace BHR
{
    public class HUDModuleUI : MonoBehaviour
    {
        [SerializeField, FoldoutGroup("Refs")] private GameObject _startAnimation;
        [SerializeField, FoldoutGroup("Refs")] private GameObject _HUDPanel;
        [SerializeField, FoldoutGroup("Refs")] private TextMeshProUGUI _chronoText;

        private void OnEnable()
        {
            GameManager.Instance.OnTimerChanged.AddListener(DisplayChrono);

            GameManager.Instance.OnLaunchLevel.AddListener(LaunchLevel);
            GameManager.Instance.OnStartLevel.AddListener(StartLevel);
        }

        private void DisplayChrono(float timeInSeconds)
        {
            _chronoText.text = UtilitiesFunctions.TimeFormat(timeInSeconds);
        }
        
        private void LaunchLevel()
        {
            TogglePanels(false);
            _startAnimation.GetComponent<StartAnimationUI>().StartAnimation();
        }
        private void StartLevel()
        {
            TogglePanels(true);
        }

        private void TogglePanels(bool hudEnabled)
        {
            _HUDPanel.SetActive(hudEnabled);
            _startAnimation.SetActive(!hudEnabled);
        }
    }
}
