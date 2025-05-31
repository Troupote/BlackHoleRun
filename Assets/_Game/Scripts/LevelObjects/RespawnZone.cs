using Unity.VisualScripting;
using UnityEngine;

namespace BHR
{
    public class RespawnZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!GameManager.Instance.IsPaused)
                GameManager.Instance.Respawning();
        }
    }
}
