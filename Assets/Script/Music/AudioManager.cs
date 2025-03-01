using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    private EventInstance musicEventInstance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There is more than one AudioManager in the scene.");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeMusic(FMODEvent.instance.Music);
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
        RuntimeManager.PlayOneShot(sound, worldPosition);
    }

    private void InitializeMusic(EventReference musicEventReference)
    {
        Debug.Log("Initializing music with event: " + musicEventReference.Path);
        musicEventInstance = RuntimeManager.CreateInstance(musicEventReference);
        if (musicEventInstance.isValid())
        {
            Debug.Log("Music event instance created successfully.");

            // Définir les attributs 3D pour l'instance de musique
            var attributes = RuntimeUtils.To3DAttributes(Vector3.zero);
            RESULT result = musicEventInstance.set3DAttributes(attributes);
            if (result != RESULT.OK)
            {
                Debug.LogError("Failed to set 3D attributes: " + result);
                return;
            }

            result = musicEventInstance.start();
            if (result != RESULT.OK)
            {
                Debug.LogError("Failed to start music event instance: " + result);
                return;
            }
            Debug.Log("Music event instance started.");

            // Vérifiez le volume de l'instance de musique
            float volume;
            result = musicEventInstance.getVolume(out volume);
            if (result != RESULT.OK)
            {
                Debug.LogError("Failed to get volume: " + result);
                return;
            }
            Debug.Log("Music event instance volume: " + volume);

            // Assurez-vous que le volume n'est pas à zéro
            if (volume == 0f)
            {
                result = musicEventInstance.setVolume(1f);
                if (result != RESULT.OK)
                {
                    Debug.LogError("Failed to set volume: " + result);
                    return;
                }
                Debug.Log("Music event instance volume set to 1.");
            }
        }
        else
        {
            Debug.LogError("Failed to create music event instance.");
        }
    }
}
