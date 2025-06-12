using Sirenix.OdinInspector;
using UnityEngine;

namespace BHR
{
    public class Checkpoint : MonoBehaviour
    {
        private void Start()
        {
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit raycast);
            transform.position = new Vector3(raycast.point.x, raycast.point.y + GameManager.Instance.GameSettings.PlayerSizeForRespawn, raycast.point.z);
        }
    }
}
