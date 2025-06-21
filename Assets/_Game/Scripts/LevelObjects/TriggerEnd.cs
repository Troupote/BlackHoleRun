using UnityEngine;

public class TriggerEnd : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Invoke(nameof(InvokeEndLevel), 0.5f);
        }
    }

    private void InvokeEndLevel()
    {
        PlanetsCollidingManager.Instance.OnLevelEnded();
    }
}
