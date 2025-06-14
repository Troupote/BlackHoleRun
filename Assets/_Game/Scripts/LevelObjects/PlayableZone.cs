using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace BHR
{
    public class PlayableZone : MonoBehaviour
    {

        private void OnTriggerExit(Collider other)
        {
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
    }
}
