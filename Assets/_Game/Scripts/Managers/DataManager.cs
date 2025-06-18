using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BHR
{
    public class DataManager : ManagerSingleton<DataManager>
    {
        [Header("Levels Data")]
        [SerializeField, Required] private string _levelsFolderPath;

        private List<LevelDataSO> _levelDatas = new List<LevelDataSO>();
        public List<LevelDataSO> LevelDatas => _levelDatas;

        public override void Awake()
        {
            base.Awake();
            FindLevelDatas();
        }

        private void FindLevelDatas()
        {
            _levelDatas = new List<LevelDataSO>();
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(LevelDataSO).Name, new[] { _levelsFolderPath });
            foreach (string guid in guids)
            {

                string path = AssetDatabase.GUIDToAssetPath(guid);
                LevelDataSO data = AssetDatabase.LoadAssetAtPath<LevelDataSO>(path);
                if (data != null)
                    _levelDatas.Add(data);
            }
            _levelDatas = _levelDatas.OrderBy(d => d.ID).ToList();
        }

        public Dictionary<int, float> GetLevelsBestTime()
        {
            Dictionary<int, float> levelsBestTime = new Dictionary<int, float>();
            foreach (LevelDataSO levelData in _levelDatas)
                levelsBestTime.Add(levelData.ID, levelData.BestTime());
            return levelsBestTime;
        }

        public int GetLastLevelCompletedID()
        {
#if UNITY_EDITOR
            if(DebugManager.Instance.UnlockedAllLevels) return int.MaxValue;
#endif
            var levelsCompleted = _levelDatas.Where(d => d.BestTime() <= d.Times[MedalsType.EARTH]).OrderBy(d => d.ID);
            return levelsCompleted.Count() > 0 ? levelsCompleted.LastOrDefault().ID : -1;
        }
    }
}
