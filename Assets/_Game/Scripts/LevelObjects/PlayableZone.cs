using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace BHR
{
    public class PlayableZone : MonoBehaviour
    {
        private bool _humanoidIsInPlayableZone = false;
        private bool _singularityIsInPlayableZone = false;

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) _humanoidIsInPlayableZone = false;
            if (other.CompareTag("Singularity")) _singularityIsInPlayableZone = false;

#if UNITY_EDITOR
            if (DebugManager.Instance.PlayableZoneDisabled)
                return;
#endif
            if (GameManager.Instance.IsPlaying && PlayableZoneManager.Instance.CanCheck)
            {
                if (other.CompareTag("Player") && GameManager.Instance.ActivePlayerState == PlayerState.HUMANOID)
                {
                    GameManager.Instance.Respawning();
                    Debug.Log("HUMANOID Respawn");
                }
                else if(other.CompareTag("Singularity") && GameManager.Instance.ActivePlayerState == PlayerState.SINGULARITY)
                {
                    GameManager.Instance.Respawning();
                    Debug.Log("SINGULARITY Respawn");
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) _humanoidIsInPlayableZone = true;
            if (other.CompareTag("Singularity")) _singularityIsInPlayableZone = true;
        }

        public void ForceCheckWithVariable()
        {
            if(!_humanoidIsInPlayableZone && !_singularityIsInPlayableZone && GameManager.Instance.IsPlaying)
                GameManager.Instance.Respawning();
        }
    }
}
