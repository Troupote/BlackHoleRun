using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace BHR
{
    public class HUDModuleUI : MonoBehaviour
    {
        [SerializeField, FoldoutGroup("Refs")] private GameObject _startAnimation;
        [SerializeField, FoldoutGroup("Refs")] private TextMeshProUGUI _chronoText;

        private void OnEnable()
        {
            GameManager.Instance.OnTimerChanged.AddListener(DisplayChrono);

            GameManager.Instance.OnLaunchLevel.AddListener(_startAnimation.GetComponent<StartAnimationUI>().StartAnimation);
        }

        private void DisplayChrono(float timeInSeconds)
        {
            _chronoText.text = UtilitiesFunctions.TimeFormat(timeInSeconds);
        }
    }
}
