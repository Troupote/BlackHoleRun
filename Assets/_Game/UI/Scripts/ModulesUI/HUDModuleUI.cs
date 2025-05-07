using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace BHR
{
    public class HUDModuleUI : MonoBehaviour
    {
        [SerializeField, FoldoutGroup("Refs")] private TextMeshProUGUI _chronoText;

        private void OnEnable()
        {
            GameManager.Instance.OnTimerChanged.AddListener(DisplayChrono);
        }

        private void DisplayChrono(float timeInSeconds)
        {
            _chronoText.text = UtilitiesFunctions.TimeFormat(timeInSeconds);
        }
    }
}
