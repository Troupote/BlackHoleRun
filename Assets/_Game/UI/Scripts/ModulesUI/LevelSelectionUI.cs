using Assets.SimpleLocalization.Scripts;
using BHR;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LevelSelectionUI : AModuleUI
{
    [SerializeField, Required] private GameObject _levelSelectionPanel;
    [SerializeField, Required] private GameObject _levelSelectedPanel;
    [SerializeField, Required] private Button _playMenuButton;

    [FoldoutGroup("Level Selection", expanded: true)]
    [SerializeField, FoldoutGroup("Level Selection/Refs")]
    private List<Transform> _levelTransform = new List<Transform>();

    [FoldoutGroup("Level Infos", expanded: true)]
    [SerializeField, Required, FoldoutGroup("Level Infos/Refs")] private TextMeshProUGUI _levelNameText;
    [SerializeField, Required, FoldoutGroup("Level Infos/Refs")] private TextMeshProUGUI _bestTimeText;
    [SerializeField, Required, FoldoutGroup("Level Infos/Refs")] private Image[] _medalsSprite;
    [SerializeField, Required, FoldoutGroup("Level Infos/Refs")] private TextMeshProUGUI[] _medalsTimeText;
    [SerializeField, Required, FoldoutGroup("Level Infos/Refs")] private Button _playButton;
    [SerializeField, Required, FoldoutGroup("Level Infos/Refs")] private GameObject _tutorielSet;
    [SerializeField, Required, FoldoutGroup("Level Infos/Refs")] private Toggle _practiceToggle;
    [SerializeField, Required, FoldoutGroup("Level Infos/Localization")] private string _notCompletedYetLocalizationKey;

    [SerializeField, Required, FoldoutGroup("Settings")] private Color _hideColor;

    private void OnEnable()
    {
        //LoadLevelsSelection();
        UnloadLevel();
        LoadLevel(GameManager.Instance.VSLevelData);
    }
    private void UnloadLevel()
    {
        _levelSelectedPanel.SetActive(false);
        _levelSelectionPanel.SetActive(true);
        ModuleManager.Instance.CanBack = true;
    }

    private void LoadLevelsSelection()
    {
        for(int i=0; i < _levelTransform.Count; i++)
        {
            LevelDataSO data = DataManager.Instance.LevelDatas[i];
            bool unlocked = data.ID - 1 <= DataManager.Instance.GetLastLevelCompletedID();

            Button button = _levelTransform[i].GetComponentInChildren<Button>();
            TextMeshProUGUI text = _levelTransform[i].GetComponentInChildren<TextMeshProUGUI>();
            Image earthMedal = _levelTransform[i].GetChild(2).GetComponent<Image>();
            Image moonMedal = _levelTransform[i].GetChild(3).GetComponent<Image>();
            Image sunMedal = _levelTransform[i].GetChild(4).GetComponent<Image>();

            button.onClick.RemoveAllListeners();
            if (unlocked) 
            {
                button.onClick.AddListener(() => ModuleManager.Instance.CanBack = false);
                button.onClick.AddListener(() => ModuleManager.Instance.SaveBackSelectable(button));
                button.onClick.AddListener(() => _playButton.Select());
                button.onClick.AddListener(() => LoadLevel(data));
                text.text = $"{data.LevelName}\n{data.ID.ToString("D2")}";

                UtilitiesFunctions.DisplayMedals((int)data.MedalObtained(), data, new Image[] { earthMedal, moonMedal, sunMedal });
            }

            earthMedal.gameObject.SetActive(unlocked);
            moonMedal.gameObject.SetActive(unlocked);
            sunMedal.gameObject.SetActive(unlocked);
            text.enabled = unlocked;
            _levelTransform[i].GetChild(5).gameObject.SetActive(!unlocked);
        }
    }

    public void LoadLevel(LevelDataSO data)
    {
        // Enable tutoriel set if level one
        _tutorielSet.SetActive(data.ID <= 1);

        _playButton.onClick.RemoveListener(() => GameManager.Instance.SaveSelectedLevel(data));
        _playButton.onClick.AddListener(() => GameManager.Instance.SaveSelectedLevel(data));

        if (data.ID <= 1)
        {
            _playButton.onClick.RemoveListener(() => GameManager.Instance.SetTutoriel(_tutorielSet.GetComponentInChildren<Toggle>().isOn));
            _playButton.onClick.AddListener(() => GameManager.Instance.SetTutoriel(_tutorielSet.GetComponentInChildren<Toggle>().isOn));
        }

        _playButton.onClick.RemoveListener(() => GameManager.Instance.IsPracticeMode = _practiceToggle.isOn);
        _playButton.onClick.AddListener(() => GameManager.Instance.IsPracticeMode = _practiceToggle.isOn);
        _practiceToggle.isOn = GameManager.Instance.IsPracticeMode;

        _levelNameText.text = $"{data.LevelName}\n{data.ID.ToString("D2")}";

        UtilitiesFunctions.DisplayMedals((int)data.MedalObtained(), data, _medalsSprite, _medalsTimeText);

        // @todo Localization
        _bestTimeText.text = LocalizationManager.Localize("Best").ToUpper() + " : " + (data.BestTime() == float.MaxValue ? LocalizationManager.Localize(_notCompletedYetLocalizationKey) : UtilitiesFunctions.TMPBalises(UtilitiesFunctions.TimeFormat(data.BestTime()), "OverpassMono-VariableFont_wght SDF", true));

        _levelSelectionPanel.SetActive(false);
        _levelSelectedPanel.SetActive(true);
    }

    //private void HandleInput(InputAction.CallbackContext ctx)
    //{
    //    if (ctx.performed && ctx.action.name == InputActions.Cancel)
    //        Back();
    //}

    public override void Back()
    {
        {
            base.Back();
            ModuleManager.Instance.OnModuleEnableWithTransition(ModuleManager.Instance.GetModule(ModuleType.MAIN_TITLE), true);
            ModuleManager.Instance.SaveBackSelectable(_playMenuButton);

        }
        ModuleManager.Instance.SelectBackSelectable();
    }
}
