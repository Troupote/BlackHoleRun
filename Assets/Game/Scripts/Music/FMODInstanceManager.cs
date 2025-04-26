using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMODInstanceManager : MonoBehaviour
{
    public static FMODInstanceManager instance { get; private set; }

    private EventInstance[] musicEventInstances;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There is more than one FMODInstanceManager in the scene.");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeMusicInstances(EventReference[] musicEvents, float volume)
    {
        musicEventInstances = new EventInstance[musicEvents.Length];
        for (int i = 0; i < musicEvents.Length; i++)
        {
            musicEventInstances[i] = RuntimeManager.CreateInstance(musicEvents[i]);
            musicEventInstances[i].setVolume(volume);
        }
    }

    public EventInstance GetMusicEventInstance(int index)
    {
        if (index >= 0 && index < musicEventInstances.Length)
        {
            return musicEventInstances[index];
        }
        else
        {
            Debug.LogError("Music event instance index is out of range.");
            return default;
        }
    }

    private void OnDestroy()
    {
        // Libérez les instances des événements FMOD
        for (int i = 0; i < musicEventInstances.Length; i++)
        {
            musicEventInstances[i].release();
        }
    }
}
