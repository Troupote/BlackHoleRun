using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BHR
{
    public class PauseModuleUI : AModuleUI
    {
        [SerializeField, FoldoutGroup("Refs")] private TextMeshProUGUI _currentLevelState;

        private void OnEnable()
        {
            _currentLevelState.text = $"Level {GameManager.Instance.CurrentLevel.ID} - {GameManager.Instance.CurrentLevel.name}\n{UtilitiesFunctions.TimeFormat(GameManager.Instance.Timer)}";
        }

        public void RestartLevel() => GameManager.Instance.RestartLevel();
        public void Resume() => GameManager.Instance.Resume();
        public void QuitLevel() => GameManager.Instance.QuitLevel();

        public override void Back()
        {
            Resume();
        }
    }
}
