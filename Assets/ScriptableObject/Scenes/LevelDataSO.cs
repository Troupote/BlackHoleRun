using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BHR
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "ScenesData/LevelData")]
    public class LevelDataSO : SceneDataSO
    {
        public string Name;
        public int ID; 
        [Header("TIMES"), ShowInInspector, ValidateInput("TimesValidate"), Tooltip("Times are in seconds")]
        public SerializedDictionary<MedalsType, float> Times = new SerializedDictionary<MedalsType, float>();


#if UNITY_EDITOR
        private bool TimesValidate(ref string errorMessage)
        {
            if(!(Times.ContainsKey(MedalsType.EARTH) && Times.ContainsKey(MedalsType.MOON) && Times.ContainsKey(MedalsType.SUN)))
            {
                errorMessage = "Dictionary must have times referenced for EARTH, MOON and SUN values";
                return false;
            }
            if(Times.Count > 6)
            {
                errorMessage = "Dictionary can have between 3 and 6 elements";
                return false;
            }
            return true;
        }
#endif
    }
}
