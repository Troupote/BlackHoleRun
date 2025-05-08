using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BHR
{
    public class InputBindingSpriteAutoUI : MonoBehaviour
    {
        [SerializeField, Required] private InputActionReference _actionRef;
        [SerializeField, Tooltip("If true, will detect active player in game rather than the allowed player")] 
        private bool _inGame = false;

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            UpdateSprite();
            if (_inGame)
                GameManager.Instance.OnMainPlayerStateChanged.AddListener((state, haveSwitched) => UpdateSprite());
            else
                PlayersInputManager.Instance.OnAllowedInputChanged.AddListener((newAllowed) => UpdateSprite());
        }

        private void OnDisable()
        {
            if (_inGame)
                GameManager.Instance.OnMainPlayerStateChanged.RemoveListener((state, haveSwitched) => UpdateSprite());
            else
                PlayersInputManager.Instance.OnAllowedInputChanged.RemoveListener((newAllowed) => UpdateSprite());
        }

        private void UpdateSprite()
        {
            // If no need to change sprite 
            if ((_inGame && GameManager.Instance.ActivePlayerState == PlayerState.UI)
                || (!_inGame && (PlayersInputManager.Instance.CurrentAllowedInput == AllowedPlayerInput.NONE || PlayersInputManager.Instance.CurrentAllowedInput == AllowedPlayerInput.BOTH)))
                return;

            var spriteFinder = PlayersInputManager.Instance.ActionsSO.BindingsControlPathToSprite;
            int playerIndex = _inGame ? GameManager.Instance.ActivePlayerIndex : (PlayersInputManager.Instance.CurrentAllowedInput == AllowedPlayerInput.FIRST_PLAYER ? 0 : 1);
            PlayerControllerState currentActivePlayerController = PlayersInputManager.Instance.PlayersControllerState[playerIndex];
            string controlPathKey = "None";

            if (currentActivePlayerController == PlayerControllerState.KEYBOARD)
                controlPathKey = _actionRef.action.bindings.FirstOrDefault(b => b.effectivePath.Contains("Keyboard") || b.effectivePath.Contains("Mouse")).effectivePath;
            else if (currentActivePlayerController == PlayerControllerState.GAMEPAD)
                controlPathKey = _actionRef.action.bindings.FirstOrDefault(b => b.effectivePath.Contains("Gamepad")).effectivePath;


            if (spriteFinder.ContainsKey(controlPathKey))
                _image.sprite = spriteFinder[controlPathKey];
            else
                Debug.LogError($"Control path {controlPathKey} isn't in the dictionary / an allowed path");
        }
    }
}
