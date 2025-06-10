using BHR;
using Sirenix.OdinInspector;
using UnityEngine;

public class SetCheckpoint : MonoBehaviour
{
    private Transform _targetCheckpoint;


#if UNITY_EDITOR
    [Button]
#endif
    private void Start()
    {
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit raycast);
        transform.position = new Vector3(raycast.point.x, raycast.point.y + GameManager.Instance.GameSettings.PlayerSizeForRespawn, raycast.point.z);

        _targetCheckpoint = transform;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(CheckpointsManager.Instance.CurrentCheckpoint != _targetCheckpoint && other.CompareTag("Player"))
            CheckpointsManager.Instance.SetCheckpoint(_targetCheckpoint);
    }
}
