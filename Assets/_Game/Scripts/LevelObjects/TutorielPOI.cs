using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BHR
{
    [Serializable]
    public struct TutorielData
    {
        public string TutorielNameKey;
        public InputActionReference ActionRef;
        public bool HasPopup;
        [ShowIf(nameof(HasPopup))]
        public string DescriptionKey;
        public Sprite Scheme;
    }
    public class TutorielPOI : MonoBehaviour
    {

        [SerializeField]
        private TutorielData _turorialData;

        [Header("Settings"), SerializeField, Tooltip("Once tutoriel popup closed, wait this time before re trigger it if detected")] private float _durationMinBetweenTriggers = 2f;
        private bool _hasAlreadyPopup = false;
        [Min(15f)]
        [SerializeField] private float _distanceFade;

        [SerializeField] private bool _hasComposite;
        [SerializeField, Required, ShowIf(nameof(_hasComposite))]
        private GameObject _uniqueBinding;
        [SerializeField, Required, ShowIf(nameof(_hasComposite))]
        private GameObject _compositeBindingsParent;

        [SerializeField]
        private bool _lastPOI = false;

        private void Awake()
        {
            foreach(FadeWithDistanceUI fade in GetComponentsInChildren<FadeWithDistanceUI>())
                fade.DistanceFade = _distanceFade;

            _hasAlreadyPopup = false;
            GameManager.Instance.CanOpenPopup = false;
        }

        private void OnEnable()
        {
            ToggleObject(GameManager.Instance.TutorielEnabled);
        }

        private void ToggleObject(bool enable)
        {
            gameObject.SetActive(enable);
        }

        private void OnTriggerEnter(Collider other)
        {
#if UNITY_EDITOR
            if (DebugManager.Instance.DisableTutorielPopup) return;
#endif
            // @todo remove tuto if level completed ? -> make a settings option ?
            if(other.CompareTag("Player") && _turorialData.HasPopup)
            {
                if (CharactersManager.Instance.isHumanoidAiming) return;
                ToggleUI(false);
                if(_hasAlreadyPopup)
                {
                    ModuleManager.Instance.transform.GetComponentInChildren<HUDModuleUI>(includeInactive: true).TogglePopup(true);
                    GameManager.Instance.SavedTutorielData = _turorialData;
                    GameManager.Instance.CanOpenPopup = true;
                }
                else
                {
                    GameManager.Instance.LoadTutorielData(_turorialData);
                    if (_lastPOI)
                        GameManager.Instance.FinishTutoriel();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ToggleUI(true);
                GameManager.Instance.CanOpenPopup = false;

                if (!_hasAlreadyPopup)
                {
                    _hasAlreadyPopup = true;
                }
                else
                    ModuleManager.Instance.transform.GetComponentInChildren<HUDModuleUI>()?.TogglePopup(false);
            }
        }

        private void Start()
        {
            //ModuleManager.Instance.OnTutorielToggled.AddListener(ToggleUI);
            if (_hasComposite)
            {
                GameManager.Instance.OnMainPlayerStateChanged.AddListener((newState, hasSwitched) => CheckComposite());
                if(PlayersInputManager.Instance.CurrentActivePlayerDevice != null)
                CheckComposite();
            }

            GameManager.Instance.OnLaunchLevel.AddListener((state) => _hasAlreadyPopup = false);
            ToggleUI(true);
        }

        private void ToggleUI(bool enabled)
        {
            transform.GetChild(0).gameObject.SetActive(enabled);
            transform.GetChild(1).gameObject.SetActive(enabled);
        }

        private void CheckComposite()
        {
            // Hard coding for displaying 4 bindings for move action
            bool displayComposite = false;
            if (PlayersInputManager.Instance.CurrentActivePlayerDevice is Keyboard || PlayersInputManager.Instance.CurrentActivePlayerDevice is Mouse)
                displayComposite = true;

            _uniqueBinding.SetActive(!displayComposite);
            _compositeBindingsParent.SetActive(displayComposite);
        }
    }

}
