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
            if(CheckpointsManager.Instance.CurrentCheckpoint != _targetCheckpoint && collision.body.CompareTag("Player") && CharactersManager.Instance.canCharacterJump)
                CheckpointsManager.Instance.SetCheckpoint(_targetCheckpoint);
        }
    }
}
