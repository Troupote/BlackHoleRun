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

        [SerializeField]
        private float _playerSizeForRespawn;
        public float PlayerSizeForRespawn => _playerSizeForRespawn;

        [SerializeField]
        private float _audioCoefWhenPaused = 0.75f;
        public float AudioCoefWhenPaused => _audioCoefWhenPaused;
    }
}
