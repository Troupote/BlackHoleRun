using UnityEngine;
using BHR;
using UnityEngine.UI;
using _Game.Scripts.Music;

namespace _Game.Scripts.UIScripts
{
    public class UiButtonsSoundsOperator : MonoBehaviour
    {
        [SerializeField] private bool isBackButton;

        private Button _button;
        private Toggle _toggle;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
                _button.onClick.AddListener(HandleClick);
            _toggle = GetComponent<Toggle>();
            if (_toggle != null)
                _toggle.onValueChanged.AddListener(HandleToggle);
        }

        private void HandleClick()
        {
            if (isBackButton && FmodEventsCreator.instance != null)
                AudioManager.Instance.PlaySFX2D(FmodEventsCreator.instance.UIReturnButton);

            if (!isBackButton && FmodEventsCreator.instance != null)
                AudioManager.Instance.PlaySFX2D(FmodEventsCreator.instance.UIForwardButton);
        }

        private void HandleToggle(bool isOn)
        {
            if (FmodEventsCreator.instance == null) return;
            var evt = isOn ? FmodEventsCreator.instance.TogglePassedTrue_sfx : FmodEventsCreator.instance.TogglePassedFalse_sfx;
            AudioManager.Instance.PlaySFX2D(evt);
        }
    }
}
