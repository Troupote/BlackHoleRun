using UnityEngine;

[CreateAssetMenu(fileName = "MusicSetupSO", menuName = "Scriptable Objects/MusicSetupSO")]
public class MusicSetupSO : ScriptableObject
{

    public AudioSource[] audioSources;

    public float Volume;

    
}
