using FMOD.Studio;
using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicSetupSO", menuName = "Scriptable Objects/MusicSetupSO")]
public class MusicSetupSO : ScriptableObject
{
    public EventReference[] musicEvents;
    public float volume = 1f;
}
