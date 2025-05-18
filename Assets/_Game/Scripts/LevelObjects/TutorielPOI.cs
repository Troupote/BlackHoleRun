using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BHR
{
    [Serializable]
    public struct TutorielData
    {
        public string TutorielName;
        public InputActionReference ActionRef;
        [TextArea]
        public string Description;
        public Sprite Scheme;
    }
    public class TutorielPOI : MonoBehaviour
    {

        [SerializeField]
        private TutorielData _turorialData;

        [Header("Settings"), SerializeField, Tooltip("Once tutoriel popup closed, wait this time before re trigger it if detected")] private float _durationMinBetweenTriggers = 2f;
        private bool _isOn = false;
        [Min(15f)]
        [SerializeField] private float _distanceFade;

        [SerializeField] private bool _hasComposite;
        [SerializeField, Required, ShowIf(nameof(_hasComposite))]
        private GameObject _uniqueBinding;
        [SerializeField, Required, ShowIf(nameof(_hasComposite))]
        private GameObject _compositeBindingsParent;

        private void Awake()
        {
            foreach(FadeWithDistanceUI fade in GetComponentsInChildren<FadeWithDistanceUI>())
                fade.DistanceFade = _distanceFade;
        }

        private void OnTriggerEnter(Collider other)
        {
            // @todo remove tuto if level completed ? -> make a settings option ?
            if(other.CompareTag("Player") && !_isOn)
            {
                _isOn = true;
                GameManager.Instance.LoadTutorielData(_turorialData);
            }
        }

        private void Start()
        {
            ModuleManager.Instance.OnTutorielToggled.AddListener(ToggleUI);
            if (_hasComposite)
            {
                GameManager.Instance.OnMainPlayerStateChanged.AddListener((newState, hasSwitched) => CheckComposite());
                CheckComposite();
            }
        }

        private void ToggleUI(bool enabled)
        {
            if (_isOn && !enabled)
                Invoke("CanTriggerAgain", _durationMinBetweenTriggers);

            foreach(Transform child in transform)
                child.gameObject.SetActive(!enabled);
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

        private void CanTriggerAgain() => _isOn = false;
    }

}
