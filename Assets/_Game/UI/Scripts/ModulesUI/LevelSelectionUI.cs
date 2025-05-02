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

    [SerializeField, Required, FoldoutGroup("Settings")] private Color _hideColor;

    private void OnEnable()
    {
        LoadLevelsSelection();
        UnloadLevel();
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

            if (unlocked) 
            {
                button.onClick.RemoveListener(() => LoadLevel(data));
                button.onClick.AddListener(() => LoadLevel(data));
                text.text = $"{data.LevelName}\n{data.ID.ToString("D2")}";

                UtilitiesFunctions.DisplayMedals((int)data.MedalObtained(), data, new Image[] { earthMedal, moonMedal, sunMedal });
            }

            button.enabled = unlocked;
            earthMedal.gameObject.SetActive(unlocked);
            moonMedal.gameObject.SetActive(unlocked);
            sunMedal.gameObject.SetActive(unlocked);
            text.enabled = unlocked;
            _levelTransform[i].GetChild(5).gameObject.SetActive(!unlocked);
        }
    }

    public void LoadLevel(LevelDataSO data)
    {
        _playButton.onClick.RemoveListener(() => GameManager.Instance.SaveSelectedLevel(data));
        _playButton.onClick.AddListener(() => GameManager.Instance.SaveSelectedLevel(data));
        _levelNameText.text = $"{data.LevelName}\n{data.ID.ToString("D2")}";

        UtilitiesFunctions.DisplayMedals((int)data.MedalObtained(), data, _medalsSprite, _medalsTimeText);

        _bestTimeText.text = "BEST : " + (data.BestTime() == float.MaxValue ? "Not completed yet" : UtilitiesFunctions.TimeFormat(data.BestTime()));

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
        if(_levelSelectedPanel.activeSelf)
        {
            UnloadLevel();
        }
        else
        {
            ModuleManager.Instance.ProcessModuleState(ModuleManager.Instance.GetModule(ModuleManager.ModuleType.MAIN_TITLE), false, true);
            ModuleManager.Instance.SaveBackSelectable(_playMenuButton);

        }
        ModuleManager.Instance.SelectBackSelectable();
    }
}
