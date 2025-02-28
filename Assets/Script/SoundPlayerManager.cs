using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

public class SoundPlayerManager : MonoBehaviour
{
    [Required]
    [SerializeField]
    private Collider startupCollider;

    [Space]

    [ValidateInput("ValidateLists", "Les listes doivent avoir la m�me longueur.")]
    [SerializeField]
    private AudioSource[] ListeSourcesAudio;

    [ValidateInput("ValidateLists", "Les listes doivent avoir la m�me longueur.")]
    [SerializeField]
    private AudioClip[] ListeDeLaMusique;

    [ValidateInput("ValidateLists", "Les listes doivent avoir la m�me longueur.")]
    [SerializeField]
    private Collider[] ListeDesColliders;


    private bool ValidateLists()
    {
        return ListeSourcesAudio.Length == ListeDeLaMusique.Length && ListeDeLaMusique.Length == ListeDesColliders.Length;
    }

    void Start()
    {
        if (startupCollider == null)
        {
            Debug.LogError("Le champ startupCollider ne peut pas �tre null.");
        }
    }

    void Update()
    {

        if (startupCollider.isTrigger)
        {
            for (int j = 0; j < ListeSourcesAudio.Length; j++)
            {
                ListeSourcesAudio[j].clip = ListeDeLaMusique[j];
                ListeSourcesAudio[0].volume = 1f;
                ListeSourcesAudio[0].Play();
                ListeSourcesAudio[1].volume = 0f;
                ListeSourcesAudio[2].volume = 0f;
                ListeSourcesAudio[3].volume = 0f;
            }
        }
        


        for (int i = 0; i < ListeDesColliders.Length; i++)
        {
            if (ListeDesColliders[i].isTrigger)
            {

            }
        }
    }
}
