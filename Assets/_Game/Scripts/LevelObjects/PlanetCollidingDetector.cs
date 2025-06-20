using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlanetCollidingDetector : MonoBehaviour
{
    [SerializeField]
    private bool m_shouldHandleDetection = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Planet") && other.transform.parent != transform.parent)
        {
            Debug.Log($"Colliding with {other.gameObject.name}");
            Vector3 contactPoint = (transform.position + other.transform.position) / 2f;

            PlanetsCollidingManager.Instance.OnPlanetCollided(contactPoint);
        }
    }
}
