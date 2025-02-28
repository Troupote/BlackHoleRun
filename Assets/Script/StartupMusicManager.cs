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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Le collider startup 1 est trigger.");
        if (!hasExecuted)
        {
            for (int j = 0; j < MusicSO.audioSources.Length; j++)
            {
                MusicSO.audioSources[j].Play();
            }

            hasExecuted = true;
        }

        


    }
}
