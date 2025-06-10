using Assets.SimpleLocalization.Scripts;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BHR
{
    public class TutorielModuleUI : AModuleUI
    {
        [SerializeField, Required, FoldoutGroup("Refs")] private TextMeshProUGUI _tutorielName;
        [SerializeField, Required, FoldoutGroup("Refs")] private TextMeshProUGUI _description;
        [SerializeField, Required, FoldoutGroup("Refs")] private Image _schema;
        [SerializeField, Required, FoldoutGroup("Refs")] private Transform _bindingsContainer;
        public override void Back()
        {
            ModuleManager.Instance.OnTutorielToggled.Invoke(false);
            GameManager.Instance.Resume();
        }

        public void LoadTutorielData(TutorielData tutoData)
        {
            ModuleManager.Instance.OnTutorielToggled.Invoke(true);
            _tutorielName.text = LocalizationManager.Localize(tutoData.TutorielNameKey);
            LoadBindings(tutoData.ActionRef);
            _description.text = LocalizationManager.Localize(tutoData.DescriptionKey);
            _schema.sprite = tutoData.Scheme;
        }

        private void LoadBindings(InputActionReference actionRef)
        {
            List<InputBinding> bindings = new List<InputBinding>();
            if (actionRef != null)
            {
                bindings = actionRef.action.bindings.ToList();
                if(!PlayersInputManager.Instance.PlayersControllerState.Contains(PlayerControllerState.GAMEPAD))
                    bindings = bindings.Where(b => b.effectivePath.Contains("Keyboard") || b.effectivePath.Contains("Mouse") || b.effectivePath.Contains("Pointer")).ToList();
                if (!PlayersInputManager.Instance.PlayersControllerState.Contains(PlayerControllerState.KEYBOARD))
                    bindings = bindings.Where(b => b.effectivePath.Contains("Gamepad") || b.effectivePath.Contains("*")).ToList();
            }

            var spriteFinder = PlayersInputManager.Instance.ActionsSO.BindingsControlPathToSprite;

            int max = Mathf.Max(bindings.Count, _bindingsContainer.childCount);

            for (int i=0; i < max; i++)
            {
                // Duplicate bindings sprite to handle the adding bindings
                if(i>=_bindingsContainer.childCount)
                    Instantiate(_bindingsContainer.GetChild(0), _bindingsContainer);
                
                // Set active false to the bindings sprite in excess
                if(i >= bindings.Count)
                {
                    _bindingsContainer.GetChild(i).gameObject.SetActive(false);
                    continue;
                }

                // Set the right sprite
                string controlPath = bindings[i].effectivePath;

                // Correct if we have to translate qwerty to azerty
                if(BindingsToSprite.IsCurrentKeyboardAzerty() && controlPath.Contains("Keyboard"))
                    controlPath = BindingsToSprite.ConvertQwertyToAzerty(controlPath);

                _bindingsContainer.GetChild(i).GetComponent<Image>().sprite = spriteFinder[controlPath];
                _bindingsContainer.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
}
