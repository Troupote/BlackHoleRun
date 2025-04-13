using System.Collections.Generic;
using UnityEngine;

namespace BHR
{
    [CreateAssetMenu(fileName = "Data", menuName = "Scenes data/Data")]
    public class ScenesDataSO : ScriptableObject
    {
        [Header("Scenes data")]
        public string TestScene;
        public string MainMenuScene;
        public List<string> LevelsScene;
    }
}

