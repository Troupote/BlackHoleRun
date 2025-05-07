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
    #endregion

    #region Medals
    [FoldoutGroup("Medals", expanded: true)]
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] Transform _medalsSpriteParent;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] private Sprite[] _medalsSprite;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] private TextMeshProUGUI _medalNameText;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] private GameObject _nextLevelPanel;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] private TextMeshProUGUI _endLevelText;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] TextMeshProUGUI _medalTimeText;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] Button _rightArrowButton;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] Button _leftArrowButton;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _hideScale;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _hideMove;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _revealMove;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _switchDuration;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private string _successLevelText;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private string _failedLevelText;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private string _soloModeLevelEndText;
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

    private void OnEndLevel(float endTime, bool newBest, bool hasPlayedSolo)
    {
        _currentLevel = GameManager.Instance.CurrentLevel;
        _endTimer = endTime;
        UpdateRunInfos(newBest);
        CreateMedals();
        UpdateEndLevelInfosText(hasPlayedSolo);
        CheckIfNextLevel();
    }

    #region Run infos UI
    private void UpdateRunInfos(bool newBest)
    {
        _levelNameText.text = $"Level {_currentLevel.ID.ToString("D2")} - {_currentLevel.LevelName}";
        _currentTimeText.text = "TIME : " + UtilitiesFunctions.TimeFormat(_endTimer);
        _bestTimeText.text = newBest ? "NEW BEST TIME !" : "BEST : " + (_currentLevel.BestTime()==float.MaxValue ? "Not completed" : UtilitiesFunctions.TimeFormat(_currentLevel.BestTime()));
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

    private void UpdateEndLevelInfosText(bool hasPlayedInSolo)
    {
        string endTextInfos = MedalObtainedId == 0 ? _failedLevelText : _successLevelText;
        if (hasPlayedInSolo)
            endTextInfos = _soloModeLevelEndText;
        _endLevelText.text = endTextInfos;
    }

    public void ChangeMedal(int move)
    {
        _currentMedalDisplayed += move;
        for(int i=0; i < _medalsSpriteParent.childCount; i++)
        {
            Transform medal = _medalsSpriteParent.GetChild(i);
            bool becomingDisplayed = _currentMedalDisplayed-1== i;
            medal.DOMove(medal.position - move * new Vector3(becomingDisplayed || _currentMedalDisplayed-move-1==i ? _revealMove : _hideMove,0f), _switchDuration);
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
        _medalNameText.text = currentMedal.ToString();

        float time = _currentLevel.Times[currentMedal];
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
        if (_currentLevel.ID <= DataManager.Instance.GetLastLevelCompletedID() && _currentLevel.ID != DataManager.Instance.LevelDatas.Last().ID)
        {
            _nextLevelPanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            _nextLevelPanel.GetComponentInChildren<Button>().onClick.AddListener(() => GameManager.Instance.SaveSelectedLevel(DataManager.Instance.LevelDatas[_currentLevel.ID + 1]));
            _nextLevelPanel.GetComponentInChildren<Button>().onClick.AddListener(() => GameManager.Instance.LaunchLevel());
            _nextLevelPanel.SetActive(true);
        }
        else
            _nextLevelPanel.SetActive(false);
    }

    public void RestartLevel() => GameManager.Instance.RestartLevel(true);
    public void QuitLevel() => GameManager.Instance.QuitLevel();

}
