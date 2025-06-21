using BHR;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class PlanetsCollidingManager : ManagerSingleton<PlanetsCollidingManager>
{
    internal PlanetSpawningController Spawner { get; private set; }

    public void SetSpawner(PlanetSpawningController a_spawner)
    {
        Spawner = a_spawner;
    }
    public void StartPlanetsMovement(float a_timer)
    {
        if (Spawner != null)
        {
            Debug.Log("Starting planets movement with timer: " + a_timer);
            Spawner.SpawnPlanets(a_timer);
        }
        else
        {
            Debug.LogError("PlanetSpawningController is not set in PlanetsCollidingManager.");
        }
    }

    public void OnPlanetCollided(Vector3 contactPoint)
    {
        Spawner.HandleCollision(contactPoint);
    }

    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    public void OnLevelEnded()
    {
        if (Spawner != null)
        {
            GameManager.Instance.StopChrono();
            Spawner.StopAllCoroutines();
            CharactersManager.Instance.ManageEnding(Spawner.GetCenter());
        }
        else
        {
            Debug.LogError("PlanetSpawningController is not set in PlanetsCollidingManager.");
        }
    }
}
