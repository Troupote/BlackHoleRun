using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

public class SoundPlayerManager : MonoBehaviour
{
    [Required]
    [SerializeField]
    private Collider LeCollider;

    [SerializeField]
    private int PartOfSong;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Le collider " + LeCollider.name + " est trigger.");

            // Jouez l'événement FMOD correspondant à la partie de la chanson
            if (PartOfSong > 0)
            {
                var musicEventInstance = FMODInstanceManager.instance.GetMusicEventInstance(PartOfSong - 1);
                if (musicEventInstance.isValid())
                {
                    // Définir les attributs 3D pour l'instance de musique
                    var attributes = RuntimeUtils.To3DAttributes(Vector3.zero);
                    musicEventInstance.set3DAttributes(attributes);

                    musicEventInstance.setVolume(1f);
                    Debug.Log("Playing part of song: " + PartOfSong);
                }
            }
            else
            {
                Debug.LogError("PartOfSong index is out of range.");
            }
        }
    }
}
