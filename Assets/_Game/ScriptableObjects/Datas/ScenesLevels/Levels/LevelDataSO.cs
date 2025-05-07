using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace BHR
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "ScenesData/LevelData")]
    public class LevelDataSO : SceneDataSO
    {
        public string LevelName;
        public int ID; 
        [Header("TIMES"), ShowInInspector, ValidateInput("TimesValidate"), Tooltip("Times are in seconds")]
        public SerializedDictionary<MedalsType, float> Times = new SerializedDictionary<MedalsType, float>();
        [Header("CONDITIONS")]
        public HoleMans StartMan = HoleMans.BLACK_HOLE_MAN;


        public bool SaveTime(float timeInSeconds)
        {
            if(timeInSeconds < BestTime())
            {
                PlayerPrefs.SetFloat(ID.ToString(), timeInSeconds);
                return true;
            }
            return false;
        }

        public MedalsType MedalObtained(float timeInSeconds) => Times.Where(t => t.Value >= timeInSeconds).OrderBy(t => t.Value).FirstOrDefault().Key;
        public MedalsType MedalObtained() => Times.Where(t => t.Value >= BestTime()).OrderBy(t => t.Value).FirstOrDefault().Key;
        public float BestTime() => PlayerPrefs.GetFloat(ID.ToString(), float.MaxValue);

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
