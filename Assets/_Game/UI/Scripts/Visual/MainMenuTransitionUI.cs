using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BHR
{
    public class MainMenuTransitionUI : MonoBehaviour
    {
        [SerializeField, Required, FoldoutGroup("Settings")] private float _duration;
        [SerializeField, Required, FoldoutGroup("Settings")] private Ease _ease;
        [SerializeField, Required, FoldoutGroup("Settings")] private SerializedDictionary<ModuleType, Quaternion> _modulesRotation = new SerializedDictionary<ModuleType, Quaternion>();

        private void Start()
        {
            GameObject.FindWithTag("MainCamera").transform.rotation = _modulesRotation[ModuleManager.Instance.ModulesRef[ModuleManager.Instance.CurrentModule]];
        }

        public void LaunchTransition(bool back) => StartCoroutine(Transition(back));

        IEnumerator Transition(bool back)
        {
            ModuleType module = ModuleManager.Instance.ModulesRef[ModuleManager.Instance.SavedModuleToLoad];
            Transform camera = GameObject.FindWithTag("MainCamera").transform;
            if(camera.rotation != _modulesRotation[module])
            {
                camera.DORotateQuaternion(_modulesRotation[module], _duration).SetEase(_ease);
                yield return new WaitForSeconds(_duration);
            }
            ModuleManager.Instance.LoadSavedModule(back);
        }
    }
}
