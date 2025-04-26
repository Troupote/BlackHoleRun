using UnityEngine;
using FMODUnity;


public class FMODEvent : MonoBehaviour
{
    [field: Header("Music")]
    [field: SerializeField] public EventReference Music { get; private set; }
    
    [field: Header("Footsteps")]
    [field: SerializeField] public EventReference Footsteps { get; private set; }
    
    public static FMODEvent instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There is more than one FMODEvent in the scene.");
        }
        instance = this;

    }
}
