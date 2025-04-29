
using UnityEngine;

public class FakeSingularityThrower : MonoBehaviour
{

   void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Simulate the singularity throw
            AudioManager.Instance.PlayOneShot(FmodEventsCreator.instance.blackHoleTrowSfx, this.transform.position);
        }
        
    }
}
