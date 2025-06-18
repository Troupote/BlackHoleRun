using Sirenix.OdinInspector;
using System.Drawing;
using UnityEditor;
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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = UnityEngine.Color.blue;
            Vector3 endPos = transform.position + transform.forward * 2f;
            Gizmos.DrawLine(transform.position, endPos);

            Handles.color = UnityEngine.Color.blue;
            Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(transform.forward), 2f, EventType.Repaint);
        }
#endif
    }
}
