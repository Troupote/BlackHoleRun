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

    public float FakeEndTime;
    public LevelDataSO FakeLevelData;

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
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] TextMeshProUGUI _medalTimeText;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] Button _rightArrowButton;
    [SerializeField, Required, FoldoutGroup("Medals/Refs")] Button _leftArrowButton;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private Color _unlockedMedalColor;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _hideScale;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private Color _hideColor;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _hideMove;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _revealMove;
    [SerializeField, Required, FoldoutGroup("Medals/Settings")] private float _switchDuration;
    private int _currentMedalId;
    #endregion

    private void Awake()
    {
        // @todo link to End level event
        UpdateRunInfos();
        CreateMedals();
    }

    #region Run infos UI
    private void UpdateRunInfos()
    {
        _levelNameText.text = $"Level {FakeLevelData.ID.ToString("D2")} - {FakeLevelData.name}";
        _currentTimeText.text = "TIME : " + TimeFormat(FakeEndTime);
        // Get from save files best time
        //_bestTimeText.text = "BEST : " + TimeFormat(player prefs ?);
    }
    #endregion

    #region Medals UI
    private void CreateMedals()
    {
        // Check medal obtained
        // @todo maybe link to the game manager to obtain medal ref
        MedalsType medalObtained = FakeLevelData.Times.Where(t => t.Value >= FakeEndTime).OrderBy(t => t.Value).FirstOrDefault().Key;
        _currentMedalId = (int)medalObtained;

        int spriteCount = 0;
        List<GameObject> unusedGO = new List<GameObject>();
        for(int i=0; i<_medalsSpriteParent.childCount; i++)
        {
            Transform medal = _medalsSpriteParent.GetChild(i);
            if (2 - medalObtained - i <= 0 && 4 - medalObtained - i >= 0)
            {
                medal.GetComponent<Image>().sprite = _medalsSprite[spriteCount];
                medal.GetComponent<Image>().color = spriteCount <= _currentMedalId ? Color.white : _hideColor;
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

    public void ChangeMedal(int move)
    {
        _currentMedalId += move;
        for(int i=0; i < _medalsSpriteParent.childCount; i++)
        {
            Transform medal = _medalsSpriteParent.GetChild(i);
            bool becomingSelected = _currentMedalId== i;
            medal.DOMove(medal.position - move * new Vector3(becomingSelected || _currentMedalId-move==i ? _revealMove : _hideMove,0f), _switchDuration);
            medal.DOScale(becomingSelected ? 1f : _hideScale, _switchDuration);
        }
        ActivateButtons();
        Invoke("ActivateButtons", _switchDuration + 0.01f);

        DisplayData();
        CheckButtons();
    }

    private void DisplayData()
    {
        MedalsType currentMedal = (MedalsType)_currentMedalId;
        _medalNameText.text = currentMedal.ToString();

        float time = FakeLevelData.Times[currentMedal];
        _medalTimeText.text = TimeFormat(time);

        _medalNameText.color = _medalTimeText.color = FakeEndTime <= time ? Color.white : _unlockedMedalColor;
    }

    private void CheckButtons()
    {
        _leftArrowButton.gameObject.SetActive(_currentMedalId > 0);
        _rightArrowButton.gameObject.SetActive(_currentMedalId < 2);
    }

    private void ActivateButtons()
    {
        _leftArrowButton.enabled = _rightArrowButton.enabled = !_rightArrowButton.enabled;
    }
    #endregion

    #region Utilities
    private string TimeFormat(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int centiseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        return string.Format("{0}:{1:00}:{2:00}", minutes, seconds, centiseconds);
    }
    #endregion
}
