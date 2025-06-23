using Assets.SimpleLocalization.Scripts;
using BHR;
using DG.Tweening;
using NUnit.Framework;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndLevelModuleUI : MonoBehaviour
{
    #region Run infos
    [FoldoutGroup("Run Infos", expanded: true)]
    [SerializeField, Required, FoldoutGroup("Run Infos/Refs")] private TextMeshProUGUI _levelNameText;
    [SerializeField, Required, FoldoutGroup("Run Infos/Refs")] private TextMeshProUGUI _currentTimeText;
    [SerializeField, Required, FoldoutGroup("Run Infos/Refs")] private TextMeshProUGUI _bestTimeText;
    [SerializeField, Required, FoldoutGroup("Run Infos/Refs")] private Toggle _practiceToggle;
    #endregion

    #region Medals
    [FoldoutGroup("Medals", expanded: true)]
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] Transform _medalsSpriteParent;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] private Sprite[] _medalsSprite;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] private TextMeshProUGUI _medalNameText;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] private Button _nextLevelPanel;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] private TextMeshProUGUI _endLevelText;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] TextMeshProUGUI _medalTimeText;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] Button _rightArrowButton;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] Button _leftArrowButton;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _hideScale;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _hideMovePercents;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _revealMovePercents;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _switchDuration;

    [SerializeField, Required, FoldoutGroup("Localization")] private string _newBestLocalizationKey;
    [SerializeField, Required, FoldoutGroup("Localization")] private string _successLocalizationKey;
    [SerializeField, Required, FoldoutGroup("Localization")] private string _failedLocalizationKey;
    [SerializeField, Required, FoldoutGroup("Localization")] private string _soloModeLocalizationKey;
    private int _medalObtainedId;
    private int _currentMedalDisplayed;

    private float _endTimer;
    private LevelDataSO _currentLevel;
    private int MedalObtainedId
    {
        get => _medalObtainedId;
        set
        {
            _medalObtainedId = value;
            _currentMedalDisplayed = Mathf.Max(1, _medalObtainedId);
        }
    }
    #endregion

    private void OnEnable()
    {
        GameManager.Instance.OnEndLevel.AddListener(OnEndLevel);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnEndLevel.RemoveListener(OnEndLevel);
    }

    private void OnEndLevel(float endTime, bool newBest, bool hasPlayedSolo, bool practiceMode)
    {
        _currentLevel = GameManager.Instance.CurrentLevel;
        _endTimer = endTime;
        UpdateRunInfos(newBest);
        CreateMedals();
        UpdateEndLevelInfosText(hasPlayedSolo, practiceMode);
        CheckIfNextLevel();
    }

    #region Run infos UI
    private void UpdateRunInfos(bool newBest)
    {
        _levelNameText.text = $"{LocalizationManager.Localize("Level")} {_currentLevel.ID.ToString("D2")} - {_currentLevel.LevelName}"; // @todo localization level name
        _currentTimeText.text = LocalizationManager.Localize("Time").ToUpper() + " : " + UtilitiesFunctions.TMPBalises(UtilitiesFunctions.TimeFormat(_endTimer), "OverpassMono-VariableFont_wght SDF", true);
        _bestTimeText.text = newBest ? LocalizationManager.Localize(_newBestLocalizationKey).ToUpper() : LocalizationManager.Localize("Best").ToUpper() + " : " + UtilitiesFunctions.TMPBalises(UtilitiesFunctions.TimeFormat(_currentLevel.BestTime()), "OverpassMono-VariableFont_wght SDF", true);
    }
    #endregion

    #region Medals UI
    private void CreateMedals()
    {
        // Check medal obtained
        MedalsType medalObtained = _currentLevel.MedalObtained(_endTimer);
        MedalObtainedId = (int)medalObtained;

        int spriteCount = 0;
        List<GameObject> unusedGO = new List<GameObject>();
        for(int i=0; i<_medalsSpriteParent.childCount; i++)
        {
            Transform medal = _medalsSpriteParent.GetChild(i);
            if (3 - _currentMedalDisplayed - i <= 0 && 5 - _currentMedalDisplayed - i >= 0)
            {
                medal.GetComponent<Image>().sprite = _medalsSprite[spriteCount];
                medal.GetComponent<Image>().color = spriteCount < MedalObtainedId ? Color.white : ModuleManager.Instance.HideMedalColor;
                spriteCount++;
            }
            else
            {
                medal.gameObject.SetActive(false);
                unusedGO.Add(medal.gameObject);
            }
        }
        foreach (GameObject obj in unusedGO) Destroy(obj);
        DisplayData();
        CheckButtons();
    }

    private void UpdateEndLevelInfosText(bool hasPlayedInSolo, bool practiceMode)
    {
        string endTextInfosKey = MedalObtainedId == 0 ? _failedLocalizationKey : _successLocalizationKey;
        if (hasPlayedInSolo && practiceMode)
            endTextInfosKey = "M/EL/PracticeSolo";
        else if (hasPlayedInSolo)
            endTextInfosKey = _soloModeLocalizationKey;
        else if (practiceMode)
            endTextInfosKey = "M/EL/Practice";
        _endLevelText.text = LocalizationManager.Localize(endTextInfosKey).ToUpper();
    }

    public void ChangeMedal(int move)
    {
        _currentMedalDisplayed += move;
        for(int i=0; i < _medalsSpriteParent.childCount; i++)
        {
            Transform medal = _medalsSpriteParent.GetChild(i);
            bool becomingDisplayed = _currentMedalDisplayed-1== i;
            float canvasWidth = ModuleManager.Instance.transform.localScale.x * ModuleManager.Instance.GetComponent<RectTransform>().rect.width;
            medal.DOMove(medal.position - move * new Vector3(becomingDisplayed || _currentMedalDisplayed-move-1==i ? _revealMovePercents : _hideMovePercents,0f) * canvasWidth, _switchDuration);
            medal.DOScale(becomingDisplayed ? 1f : _hideScale, _switchDuration);
        }
        ActivateButtons();
        Invoke("ActivateButtons", _switchDuration + 0.01f);

        DisplayData();
        CheckButtons();
    }

    private void DisplayData()
    {
        MedalsType currentMedal = (MedalsType)_currentMedalDisplayed;
        _medalNameText.text = LocalizationManager.Localize(UtilitiesFunctions.ToLowerWithFirstUpper(currentMedal.ToString())).ToUpper();

        float time = 0f;
        if(_currentLevel.Times.ContainsKey(currentMedal))
            time = _currentLevel.Times[currentMedal];
        _medalTimeText.text = UtilitiesFunctions.TimeFormat(time);

        _medalNameText.color = _medalTimeText.color = _endTimer <= time ? Color.white : ModuleManager.Instance.HideMedalTextColor;
    }

    private void CheckButtons()
    {
        _leftArrowButton.gameObject.SetActive(_currentMedalDisplayed > 1);
        _rightArrowButton.gameObject.SetActive(_currentMedalDisplayed < 3);
    }

    private void ActivateButtons()
    {
        _leftArrowButton.enabled = _rightArrowButton.enabled = !_rightArrowButton.enabled;
    }
    #endregion

    private void CheckIfNextLevel()
    {
        _nextLevelPanel.onClick.RemoveAllListeners();
        if (_currentLevel.ID <= DataManager.Instance.GetLastLevelCompletedID() && _currentLevel.ID != DataManager.Instance.LevelDatas.Last().ID)
        {
            _nextLevelPanel.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Localize("M/EL/NextLevel");
            _nextLevelPanel.onClick.AddListener(() => GameManager.Instance.SaveSelectedLevel(DataManager.Instance.LevelDatas[_currentLevel.ID + 1]));
            _nextLevelPanel.onClick.AddListener(() => GameManager.Instance.LaunchLevel());
        }
        else
        {
            _nextLevelPanel.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Localize("M/MT/Credits");
            _nextLevelPanel.onClick.AddListener(() =>
            {
                ModuleManager.Instance.ClearNavigationHistoric();
                ModuleManager.Instance.SetModuleToLoad(ModuleManager.Instance.GetModule(ModuleType.CREDITS));
                ScenesManager.Instance.ChangeScene(ScenesManager.Instance.MenuScene);
            });
        }
    }

    public void RestartLevel()
    {
        GameManager.Instance.IsPracticeMode = _practiceToggle.isOn;
        GameManager.Instance.RestartLevel(true);
    }
    public void QuitLevel() => GameManager.Instance.QuitLevel();

}
