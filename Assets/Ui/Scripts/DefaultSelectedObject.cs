using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BHR
{
    public class DefaultSelectedObject : MonoBehaviour
    {

        [SerializeField] private bool _selectedByDefault = true;
        [SerializeField] private bool _selectedOnEnable = false;
        [SerializeField] private bool _selectedOnDisable = false;
        [SerializeField, ShowIf(nameof(_selectedOnDisable))] private bool _onlyIfSelected = false;
        [SerializeField, Tooltip("If empty, this object itself"), ShowInInspector] private Selectable _selectedObject;

        private void Awake()
        {
            if(TryGetComponent<Selectable>(out Selectable s) && _selectedObject == null)
                _selectedObject = s;
        }
        private void OnEnable()
        {
            if(_selectedByDefault)
                ModuleManager.Instance.OnModuleEnabled.AddListener(OnModuleEnable);
            if (_selectedOnEnable)
                _selectedObject.Select();
        }

        private void OnDisable()
        {
            if(_selectedByDefault)
                ModuleManager.Instance.OnModuleEnabled.RemoveListener(OnModuleEnable);
            if (_selectedOnDisable && (!_onlyIfSelected || EventSystem.current.currentSelectedGameObject == gameObject))
                _selectedObject.Select();
        }
        private void OnModuleEnable(GameObject module, bool withBack)
        {
            if (withBack)
                return;

            _selectedObject.Select();
        }
    }

}
