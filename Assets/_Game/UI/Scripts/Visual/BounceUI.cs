using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace BHR
{
    public class BounceUI : MonoBehaviour
    {
        [SerializeField, Tooltip("Object will travel this distance in the chosen axes then return the same distance")] private float _distance;
        [SerializeField, Tooltip("Duration of one way")] private float _duration = 0.8f;
        [SerializeField, Tooltip("Axe to follow"), EnumToggleButtons] private Axes _axes;
        [SerializeField] Ease _ease;

        private void Start()
        {
            StartCoroutine(Bouncing(true));
        }

        IEnumerator Bouncing(bool first)
        {
            float distance = _distance * (first ? 1 : -1);
            Vector3 targetPos = transform.position;
            if((_axes & Axes.X) != 0) targetPos.x += distance;
            if((_axes & Axes.Y) != 0) targetPos.y += distance;
            if((_axes & Axes.Z) != 0) targetPos.z += distance;

            transform.DOMove(targetPos, _duration).SetEase(_ease);
            yield return new WaitForSeconds(_duration);

            StartCoroutine(Bouncing(!first));
        }
    }
}
