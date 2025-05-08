using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace BHR
{
    [CreateAssetMenu(fileName = "SceneData", menuName = "ScenesData/SceneData")]
    public class SceneDataSO : ScriptableObject
    {
        [Header("GLOBAL")]
        public string SceneName;
    }
}
