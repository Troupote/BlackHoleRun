using Sirenix.OdinInspector;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BHR
{
    public class ControllerSelectionModuleUI : AModuleUI
    {
        [SerializeField, Required, FoldoutGroup("Refs")] GameObject[] _inputJoinTexts;
        [SerializeField, Required, FoldoutGroup("Refs")] GameObject[] _inputReconnectIcons;
        [SerializeField, Required, FoldoutGroup("Refs")] GameObject _readyPanel;
        [SerializeField, Required, FoldoutGroup("Refs")] GameObject[] _playerStatePanels;
        [SerializeField, Required, FoldoutGroup("Refs")] GameObject _keyboardSwitchIcon;
        [SerializeField, Required, FoldoutGroup("Refs")] GameObject _gamepadSwitchIcon;
        [SerializeField, Required, FoldoutGroup("Refs")] GameObject _soloConfirmPanel;
        [SerializeField, Required, FoldoutGroup("Refs")] GameObject[] _soloIcons;
        [SerializeField, Required, FoldoutGroup("Settings")] Color _readyColor;
        [SerializeField, Required, FoldoutGroup("Settings")] Color _notReadyColor;

        private void Start()
        {
            PlayersInputManager.Instance.OnSoloModeToggle.AddListener(UpdateSoloModePanel);
            PlayersInputManager.Instance.OnPlayerReadyStateChanged.AddListener(UpdatePlayerStatePanels);
            PlayersInputManager.Instance.OnPlayersSwitch.AddListener(SwitchPlayerPanel);
        }

        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            UpdateBothPlayerStatePanels();
            UpdateSoloModePanel(false);
            for (int i = 0; i < 2; i++)
                _readyPanel.transform.GetChild(i).GetComponent<Image>().color = _notReadyColor;
        }

        public override void Back()
        {
            if(GameManager.Instance.IsPaused)
                GameManager.Instance.Resume();
            else
                base.Back();
        }

        private void UpdatePlayerStatePanels(PlayerReadyState state, int playerIndex)
        {
            int index = PlayerIndexUI(playerIndex);


            int targetPanel = 0;
            switch(state)
            {
                case PlayerReadyState.LOGGED_OUT: targetPanel = 1; UpdateReconnectIcons(index); break;
                case PlayerReadyState.NONE: break;
                case PlayerReadyState.READY:
                case PlayerReadyState.CONNECTED: targetPanel = 2 + (PlayersInputManager.Instance.PlayersControllerState[playerIndex] == PlayerControllerState.GAMEPAD ? 1 : 0); break;
            }

            for (int i = index * _playerStatePanels.Length / 2; i < _playerStatePanels.Length / (2 - index); i++)
            {
                _playerStatePanels[i].SetActive(i == targetPanel + index * _playerStatePanels.Length/2);
            }


            // Ready panel setup
            _readyPanel.SetActive(PlayersInputManager.Instance.PlayerConnectedCount() > 0);
            _readyPanel.transform.GetChild(index).GetComponent<Image>().color = state == PlayerReadyState.READY ? _readyColor : _notReadyColor;

            // Switch input indicaiton 
            _keyboardSwitchIcon.SetActive(PlayersInputManager.Instance.PlayersControllerState.Contains(PlayerControllerState.KEYBOARD));
            _gamepadSwitchIcon.SetActive(PlayersInputManager.Instance.PlayersControllerState.Contains(PlayerControllerState.GAMEPAD));
        }

        private void UpdateSoloModePanel(bool enabled)
        {
            _soloConfirmPanel.SetActive(enabled);
            _readyPanel.SetActive(!enabled);
            int soloPlayerIndex = Array.IndexOf(PlayersInputManager.Instance.PlayersReadyState, PlayerReadyState.READY);
            if(enabled)
                UpdateGameObjectInArray(_soloIcons, PlayersInputManager.Instance.PlayersControllerState[soloPlayerIndex] == PlayerControllerState.GAMEPAD);
        }

        private void UpdateBothPlayerStatePanels(bool switched = false)
        {
            int id = switched ? 1 : 0;
            UpdatePlayerStatePanels(PlayersInputManager.Instance.PlayersReadyState[id], 0);
            UpdatePlayerStatePanels(PlayersInputManager.Instance.PlayersReadyState[1-id], 1);
        }

        private void UpdateReconnectIcons(int index)
        {
            bool tkt = PlayersInputManager.Instance.PlayersControllerState[Array.IndexOf(PlayersInputManager.Instance.PlayersReadyState, PlayersInputManager.Instance.PlayersReadyState.First(p => p == PlayerReadyState.LOGGED_OUT))] == PlayerControllerState.GAMEPAD;
            UpdateGameObjectInArray(_inputReconnectIcons, tkt, index*2);
        }

        private void UpdateGameObjectInArray(GameObject[] array, bool enableGamepad, int start = 0)
        {
            int variation = enableGamepad ? 1 : 0;
            array[start + variation].SetActive(true);
            array[start+1-variation].SetActive(false);
        }

        private void SwitchPlayerPanel(bool isSwitched)
        {
            // Switch panel
            int savedLeftPanel = 0, savedRightPanel = 0;
            for (int i = 0; i < _playerStatePanels.Length; i++)
                if (_playerStatePanels[i].activeSelf)
                {
                    if (i < _playerStatePanels.Length / 2)
                        savedLeftPanel = i;
                    else
                        savedRightPanel = i;
                    _playerStatePanels[i].SetActive(false);
                }


            for (int i = 0; i < _playerStatePanels.Length; i++)
                _playerStatePanels[i].SetActive(i== _playerStatePanels.Length / 2 + savedLeftPanel || i== savedRightPanel - _playerStatePanels.Length / 2);

            if(savedLeftPanel==1)
                UpdateReconnectIcons(1);
            if(savedRightPanel==5)
                UpdateReconnectIcons(0);

            // Switch ready state
            Image leftReadyImage = _readyPanel.transform.GetChild(0).GetComponent<Image>();
            Image rightReadyImage = _readyPanel.transform.GetChild(1).GetComponent<Image>();
            Color leftColor = leftReadyImage.color;
            Color rightColor = rightReadyImage.color;

            leftReadyImage.color = rightColor;
            rightReadyImage.color = leftColor;
        }

        private int PlayerIndexUI(int trueIndex) => PlayersInputManager.Instance.IsSwitched ? 1-trueIndex : trueIndex;
    }

}
