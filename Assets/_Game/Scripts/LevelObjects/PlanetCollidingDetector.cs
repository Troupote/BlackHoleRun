using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlanetCollidingDetector : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Planet")) return;

        var contact = collision.contacts[0];
        Vector3 contactPoint = contact.point;

        PlanetsCollidingManager.Instance.OnPlanetCollided(contactPoint);
    }

}
