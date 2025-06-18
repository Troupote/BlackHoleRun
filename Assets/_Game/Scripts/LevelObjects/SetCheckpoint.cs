using Sirenix.OdinInspector;
using UnityEngine;

namespace BHR
{
    public class SetCheckpoint : MonoBehaviour
    {
        [SerializeField, Required]
        private Transform _targetCheckpoint;

        private void OnCollisionEnter(Collision collision)
        {
            if(CheckpointsManager.Instance.CurrentCheckpoint != _targetCheckpoint && collision.body.CompareTag("Player") && CharactersManager.Instance.isCharacterGrounded)
                CheckpointsManager.Instance.SetCheckpoint(_targetCheckpoint);
        }
    }
}
