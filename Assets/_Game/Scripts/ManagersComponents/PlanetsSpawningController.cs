using BHR;
using UnityEngine;
using System.Collections;

public class PlanetSpawningController : MonoBehaviour
{
    [SerializeField]
    private GameObject m_planetsObject;
    [SerializeField] 
    private GameObject m_shockwavePrefab;

    [SerializeField]
    private Transform m_planetPlacement1;
    [SerializeField]
    private Transform m_planetPlacement2;

    private Vector3 m_startPos1;
    private Vector3 m_startPos2;
    private Coroutine m_movementCoroutine;
   
    private SphereCollider m_sphereCollider1;
    private SphereCollider m_sphereCollider2;

    private void Start()
    {
        m_planetsObject.SetActive(false);
        PlanetsCollidingManager.Instance.SetSpawner(this);

        m_sphereCollider1 = m_planetPlacement1.GetComponentInChildren<SphereCollider>();
        m_sphereCollider2 = m_planetPlacement2.GetComponentInChildren<SphereCollider>();
    }

    public void SpawnPlanets(float movementDuration)
    {
        m_planetsObject.SetActive(true);

        m_startPos1 = m_planetPlacement1.position;
        m_startPos2 = m_planetPlacement2.position;

        if (m_movementCoroutine != null)
            StopCoroutine(m_movementCoroutine);

        m_movementCoroutine = StartCoroutine(MovePlanets(m_startPos1, m_startPos2, movementDuration));
    }

    private IEnumerator MovePlanets(Vector3 from1, Vector3 from2, float duration)
    {
        float timer = 0f;

        float radius1 = m_sphereCollider1.radius * m_sphereCollider1.transform.lossyScale.x;
        float radius2 = m_sphereCollider2.radius * m_sphereCollider2.transform.lossyScale.x;

        float contactDistance = radius1 + radius2;
        Vector3 direction = (from2 - from1).normalized;

        Vector3 to1 = from1 + direction * (Vector3.Distance(from1, from2) - contactDistance) / 2f;
        Vector3 to2 = from2 - direction * (Vector3.Distance(from1, from2) - contactDistance) / 2f;

        while (timer < duration)
        {
            if (!GameManager.Instance.IsPaused)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);

                m_planetPlacement1.position = Vector3.Lerp(from1, to1, t);
                m_planetPlacement2.position = Vector3.Lerp(from2, to2, t);
            }
            yield return null;
        }

        m_planetPlacement1.position = to1;
        m_planetPlacement2.position = to2;
    }

    public void ResetPlanets()
    {
        m_planetPlacement1.transform.position = m_startPos1;
        m_planetPlacement2.transform.position = m_startPos2;
    }

    public void HandleCollision(Vector3 contactPoint)
    {
        if (m_movementCoroutine != null)
        {
            StopCoroutine(m_movementCoroutine);
            m_movementCoroutine = null;
        }

        Debug.Log("Planet collision detected!");

        CameraManager.Instance.ShakeCamera(8f, 5f);
        Instantiate(m_shockwavePrefab, contactPoint, Quaternion.identity);
    }
}
