using Sirenix.OdinInspector;
using System.Collections.Generic;
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
        [SerializeField, ShowIf(nameof(_hasCompositeElement))]
        private bool _useComposite;
        protected enum DirectionComposite {NONE, up, right, down, left}
        [SerializeField, ShowIf(nameof(_useComposite)), ValidateInput(nameof(_directionIsNotNone))]
        private DirectionComposite _directionOfComposite = DirectionComposite.NONE;
        public InputBindingComposite test;

        private Image _image;

#if UNITY_EDITOR
        [SerializeField]
        private bool _showBindingsAssociatedWithAction = false;
#endif

        #region Odin stuff
        private bool _hasCompositeElement => _actionRef.action.bindings.Any(b => b.isComposite);
        private bool _directionIsNotNone => _directionOfComposite != DirectionComposite.NONE;
        #endregion

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

            List<InputBinding> controlPaths = new List<InputBinding>();
            if (currentActivePlayerController == PlayerControllerState.KEYBOARD)
                controlPaths = _actionRef.action.bindings.Where(b => b.effectivePath.Contains("Keyboard") || b.effectivePath.Contains("Mouse") || b.effectivePath.Contains("Pointer")).ToList();
            else if (currentActivePlayerController == PlayerControllerState.GAMEPAD)
                controlPaths = _actionRef.action.bindings.Where(b => b.effectivePath.Contains("Gamepad") || b.effectivePath.Contains("*")).ToList();

            string controlPathKey = controlPaths[0].effectivePath;

            if (_useComposite && _directionIsNotNone)
                controlPathKey = controlPaths.FirstOrDefault(b => b.isPartOfComposite && b.name == _directionOfComposite.ToString()).effectivePath;

            if(controlPathKey == null || controlPathKey == string.Empty || !spriteFinder.ContainsKey(controlPathKey))
            {
                Debug.LogError($"Control path {controlPathKey} isn't in the dictionary / an allowed path on {gameObject.name}");
                controlPathKey = "None";
            }

            if(currentActivePlayerController == PlayerControllerState.KEYBOARD && IsCurrentKeyboardAzerty())
                controlPathKey = ConvertQwertyToAzerty(controlPathKey);


            if (spriteFinder.ContainsKey(controlPathKey))
                _image.sprite = spriteFinder[controlPathKey];

#if UNITY_EDITOR
            if(_showBindingsAssociatedWithAction)
                foreach (InputBinding binding in _actionRef.action.bindings)
                    if(!_useComposite || binding.isPartOfComposite && binding.name == _directionOfComposite.ToString())
                        Debug.Log(binding.effectivePath);
#endif
        }

        private string ConvertQwertyToAzerty(string controlPath)
        {
            string input = controlPath.Split("/")[controlPath.Split("/").Length - 1];
            if (!_qwertyToAzerty.ContainsKey(input))
                return controlPath;
            else
            {
                string newControlPath = "";
                for (int i = 0; i < controlPath.Split("/").Length; i++)
                    if (i == controlPath.Split("/").Length - 1)
                        newControlPath += _qwertyToAzerty[input];
                    else
                        newControlPath += controlPath.Split("/")[i] + "/";
                return newControlPath;
            }
        }

        private Dictionary<string, string> _qwertyToAzerty = new Dictionary<string, string>
        {
            // Lettres différentes
            { "a", "q" },
            { "q", "a" },
            { "w", "z" },
            { "z", "w" },
            { "m", ";" },

            // Chiffres avec shift (ex: 1 = !)
            //{ "1", "&" },
            //{ "2", "é" },
            //{ "3", "\"" },
            //{ "4", "'" },
            //{ "5", "(" },
            //{ "6", "-" },
            //{ "7", "è" },
            //{ "8", "_" },
            //{ "9", "ç" },
            //{ "0", "à" },

            // Symboles
            { "-", ")" },
            { "=", "=" },  // Pas toujours mappé différemment, dépend du clavier
            { "[", "^" },
            { "]", "$" },
            { ";", "m" },  // Inverse de "m" ci-dessus
            { "'", "ù" },
            { ",", "," },  // Virgule reste
            { ".", ":" },
            { "/", "!" },
            { "\\", "*" },
            { "`", "²" }
        };

        private bool IsCurrentKeyboardAzerty() => Keyboard.current.wKey.displayName.ToLower() == "z";
    }
}
