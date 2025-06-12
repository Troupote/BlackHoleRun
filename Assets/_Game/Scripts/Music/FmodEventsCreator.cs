using UnityEngine;
using FMODUnity;
using Sirenix.OdinInspector;

public class FmodEventsCreator : MonoBehaviour
{
    [field: FoldoutGroup("SFX"), Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootsetps { get; private set; }

    [field: FoldoutGroup("Ambiance"), Header("Wind Ambient")]
    [field: SerializeField] public EventReference windAmbient { get; private set; }

    [field: FoldoutGroup("SFX"), Header("Throw Black Hole")]
    [field: SerializeField] public EventReference blackHoleTrowSfx { get; private set; }

    [field: FoldoutGroup("SFX"), Header("Level Start")]
    [field: SerializeField] public EventReference levelStart { get; private set; }

    [field: FoldoutGroup("SFX"), Header("Exit Singularity")]
    [field: SerializeField] public EventReference exitSingularity { get; private set; }

    [field: FoldoutGroup("SFX"), Header("Standby Singularity")]
    [field: SerializeField] public EventReference standbySingularity { get; private set; }

    [field: FoldoutGroup("SFX"), Header("Jump")]
    [field: SerializeField] public EventReference jump { get; private set; }

    [field: FoldoutGroup("SFX"), Header("Dash")]
    [field: SerializeField] public EventReference dash { get; private set; }

    [field: FoldoutGroup("Music"), Header("Music Beat Rigolo")]
    [field: SerializeField] public EventReference musicBeatRigolo { get; private set; }

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

