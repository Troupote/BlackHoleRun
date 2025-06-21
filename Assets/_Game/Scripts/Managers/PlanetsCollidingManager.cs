using BHR;
using System.Collections;
using UnityEngine;

public class PlanetsCollidingManager : ManagerSingleton<PlanetsCollidingManager>
{
    private PlanetSpawningController m_spawner;

    public void SetSpawner(PlanetSpawningController a_spawner)
    {
        m_spawner = a_spawner;
    }
    public void StartPlanetsMovement(float a_timer)
    {
        if (m_spawner != null)
        {
            Debug.Log("Starting planets movement with timer: " + a_timer);
            m_spawner.SpawnPlanets(a_timer);
        }
        else
        {
            Debug.LogError("PlanetSpawningController is not set in PlanetsCollidingManager.");
        }
    }

    public void OnPlanetCollided(Vector3 contactPoint)
    {
        m_spawner?.HandleCollision(contactPoint);
    }
}
