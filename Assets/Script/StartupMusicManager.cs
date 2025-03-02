using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

public class StartupMusicManager : MonoBehaviour
{
    [Required]
    [SerializeField]
    private Collider startupCollider;

    [Space]

    private bool hasExecuted = false;

    [SerializeField]
    private MusicSetupSO MusicSO;

    private void Start()
    {
        // Initialisez les instances des événements FMOD dans le gestionnaire centralisé
        FMODInstanceManager.instance.InitializeMusicInstances(MusicSO.musicEvents, MusicSO.volume);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasExecuted)
        {
            Debug.Log("Le collider startup est trigger.");

            // Définir les attributs 3D pour chaque instance d'événement FMOD
            var attributes = RuntimeUtils.To3DAttributes(Vector3.zero);
            for (int j = 0; j < MusicSO.musicEvents.Length; j++)
            {
                var musicEventInstance = FMODInstanceManager.instance.GetMusicEventInstance(j);
                musicEventInstance.set3DAttributes(attributes);
                musicEventInstance.start();
                musicEventInstance.setVolume(0f);
            }

            FMODInstanceManager.instance.GetMusicEventInstance(0).setVolume(1f);

            hasExecuted = true;
        }
    }
}
