using UnityEngine;

namespace BHR
{
    public class ForceRespawnOnTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if(GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

#if UNITY_EDITOR
            if (DebugManager.Instance.Immortal)
                return;
#endif

            if (other.CompareTag("Player") && GameManager.Instance.ActivePlayerState == PlayerState.HUMANOID || other.CompareTag("Singularity") && GameManager.Instance.ActivePlayerState == PlayerState.SINGULARITY)
                GameManager.Instance.Respawning();
        }
    }
}
