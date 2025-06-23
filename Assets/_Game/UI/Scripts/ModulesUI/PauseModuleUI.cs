using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BHR
{
    public class PauseModuleUI : AModuleUI
    {
        [SerializeField, FoldoutGroup("Refs")] private TextMeshProUGUI _currentLevelState;
        [SerializeField, FoldoutGroup("Refs")] private Toggle _practiceToggle;

        private void OnEnable()
        {
            _practiceToggle.isOn = GameManager.Instance.IsPracticeMode;

            _currentLevelState.text = $"Level {GameManager.Instance.CurrentLevel.ID} - {GameManager.Instance.CurrentLevel.name}" +
                $"\n{UtilitiesFunctions.TMPBalises(UtilitiesFunctions.TimeFormat(GameManager.Instance.Timer), "OverpassMono-VariableFont_wght SDF", true)}";
        }

        public void RestartLevel()
        {
            GameManager.Instance.IsPracticeMode = _practiceToggle.isOn;
            GameManager.Instance.RestartLevel(true);
        }
        public void Resume() => GameManager.Instance.Resume();
        public void QuitLevel() => GameManager.Instance.QuitLevel();

        public override void Back()
        {
            Resume();
        }
    }
}
