using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace BHR
{
    public class LookAt : MonoBehaviour
    {
        [SerializeField, Tooltip("Leave empty for default. (MainCamera)")]
        private Transform target;
        private Transform _target;

        [SerializeField] private bool _invert;

        private void Start()
        {
            if(target == null)
                _target = GameObject.FindWithTag("MainCamera").transform;
        }

        void LateUpdate()
        {
            if (_target == null) return;

            Vector3 direction = transform.position - _target.position;
            if (_invert) direction *= -1f;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion rotation = Quaternion.LookRotation(direction); 
                transform.rotation = rotation;
            }
        }
    }
}
