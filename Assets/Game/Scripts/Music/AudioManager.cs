using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Tia tout chier debilus retire le component AudioManager de tous les objets et ne le met que sur un seul objet");
        }
        Instance = this;
    }

    public void PlayOneShot(EventReference soundReference, Vector3 position)
    {
        RuntimeManager.PlayOneShot(soundReference, position);
        Debug.LogAssertion("PlayOneShot " + soundReference + " at " + position);
    }
}
