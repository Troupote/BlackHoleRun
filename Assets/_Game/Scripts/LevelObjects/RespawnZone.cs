using UnityEngine;

namespace BHR
{
    public class RespawnZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // @todo process player "death" -> malus ? etc
            CheckpointsManager.Instance.RespawnPlayer();
        }
    }
}
