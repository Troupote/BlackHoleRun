using UnityEngine;
using FMODUnity;

public class FmodEventsCreator : MonoBehaviour
{
    [field: Header("Throw Black Hole")]
    [field: SerializeField] public EventReference blackHoleTrowSfx {get; private set;}
    
    public static FmodEventsCreator instance;
    
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Tia tout chier debilus retire le component FmodEventCreator de tous les objets et ne le met que sur un seul objet");
        }
        instance = this;
    }
}