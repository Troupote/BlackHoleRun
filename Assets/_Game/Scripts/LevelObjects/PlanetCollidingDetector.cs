using UnityEngine;

public class PlanetCollidingDetector : MonoBehaviour
{
    [SerializeField]
    private bool m_shouldHandleDetection = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Planet"))
        {
            Vector3 contactPoint = (transform.position + other.transform.position) / 2f;

            PlanetsCollidingManager.Instance.OnPlanetCollided(contactPoint);
        }
    }
}
