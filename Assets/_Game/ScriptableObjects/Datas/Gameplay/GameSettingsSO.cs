using Sirenix.OdinInspector;
using UnityEngine;

namespace BHR
{
    [CreateAssetMenu(fileName = "GameSettingsData", menuName = "Gameplay/GameSettings")]
    public class GameSettingsSO : ScriptableObject
    {
        [Header("InGame"), SerializeField]
        private float _respawningDuration;
        public float RespawningDuration => _respawningDuration;
    }
}
