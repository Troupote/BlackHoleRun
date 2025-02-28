using DG.Tweening;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

public class SoundPlayerManager : MonoBehaviour
{
    [Required]
    [SerializeField]
    private Collider LeCollider;

    [SerializeField]
    private MusicSetupSO MusicSO;

    [SerializeField]
    private int PartOfSong;
  
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Le collider " + LeCollider.name + " est trigger.");
        MusicSO.audioSources[PartOfSong - 1].DOFade(1f, 0.8f);
    }
}
