using UnityEngine;

namespace BHR
{
    public class RespawnZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other) => GameManager.Instance.Respawning();
    }
}
