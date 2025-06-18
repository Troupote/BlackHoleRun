using BHR;
using System.Collections;
using UnityEngine;

public class PlanetsCollidingManager : ManagerSingleton<PlanetsCollidingManager>
{
    [SerializeField]
    private Transform planetPlacement1;

    [SerializeField]
    private Transform planetPlacement2;

    [SerializeField] 
    private GameObject m_shockwavePrefab;


    private float m_timerBeforeColliding = 20.0f;

    private Vector3 m_startPos1;
    private Vector3 m_startPos2;

    private Coroutine m_movementCoroutine;

    public void SetPlanetCollidingTimer(float a_timer, bool a_shouldStartImmediately = false)
    {
        m_timerBeforeColliding = a_timer;

        if (a_shouldStartImmediately)
        {
            StartPlanetCollidingTimer();
        }
    }

    public void StartPlanetCollidingTimer()
    {
        if (m_timerBeforeColliding > 0f && m_movementCoroutine == null)
        {
            m_startPos1 = planetPlacement1.position;
            m_startPos2 = planetPlacement2.position;

            m_movementCoroutine = StartCoroutine(HandlePlanetsMovement());
        }
    }

    private IEnumerator HandlePlanetsMovement()
    {
        float elapsedTime = 0f;

        var endPos1 = (m_startPos1 + m_startPos2) / 2f;
        var endPos2 = endPos1;

        while (elapsedTime < m_timerBeforeColliding)
        {
            while (GameManager.Instance.IsPaused)
            {
                yield return null;
            }

            float t = elapsedTime / m_timerBeforeColliding;

            planetPlacement1.position = Vector3.Lerp(m_startPos1, endPos1, t);
            planetPlacement2.position = Vector3.Lerp(m_startPos2, endPos2, t);

            elapsedTime += Time.deltaTime * GameManager.Instance.GameTimeScale;

            yield return null;
        }

        planetPlacement1.position = endPos1;
        planetPlacement2.position = endPos2;
    }
    public void OnPlanetCollided(Vector3 contactPoint)
    {
        if (m_movementCoroutine != null)
        {
            StopCoroutine(m_movementCoroutine);
            m_movementCoroutine = null;
        }

        Debug.Log("Planets have collided!");

        CameraManager.Instance.ShakeCamera(8f, 5f);

        GameObject shockwave = Instantiate(m_shockwavePrefab, contactPoint, Quaternion.identity);

        //shockwave.transform.localScale = Vector3.one * averagePlanetScale; // Start big!



        //Instantiate(m_vfx, contactPoint, Quaternion.identity);
    }
}
