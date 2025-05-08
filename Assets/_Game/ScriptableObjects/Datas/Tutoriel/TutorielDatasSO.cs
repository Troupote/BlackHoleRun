using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace BHR
{
    [CreateAssetMenu(fileName = "TutorielDatas", menuName = "Gameplay/TutorielDatasSO")]
    public class TutorielDatasSO : ScriptableObject
    {
        [SerializeField, Required]
        private SerializedDictionary<InputActionReference, TutorielData> _actionRefTutoriels;
        public Dictionary<InputActionReference, TutorielData> ActionRefTutoriels => _actionRefTutoriels;
    }

    [Serializable]
    public struct TutorielData
    {
        public string TutorielName;
        [TextArea]
        public string Description;
        public Sprite Scheme;
    }
}
