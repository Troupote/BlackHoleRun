using UnityEngine;
using FMODUnity;

public class FmodEventsCreator : MonoBehaviour
{
    [field: Header("Player SFX")]
	[field: SerializeField] public EventReference playerFootsetps {get; private set;}


	[field: Header("Throw Black Hole")]
    [field: SerializeField] public EventReference blackHoleTrowSfx {get; private set;}
    
    [field: Header("Level Start")]
    [field: SerializeField] public EventReference levelStart {get; private set;}
    
    [field: Header("Exit Singularity")]
    [field: SerializeField] public EventReference exitSingularity {get; private set;}
    
    [field: Header("Standby Singularity")]
    [field: SerializeField] public EventReference standbySingularity {get; private set;}
    
    [field: Header("Jump")]
    [field: SerializeField] public EventReference jump {get; private set;}
    
    [field: Header("Dash")]
    [field: SerializeField] public EventReference dash {get; private set;}
    
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