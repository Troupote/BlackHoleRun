using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace BHR
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] public float MasterVolume => SettingsSave.LoadMasterVolume();
        [SerializeField] public float SFXVolume => SettingsSave.LoadSoundsVolume() * MasterVolume;
        [SerializeField] public float MusicVolume => SettingsSave.LoadMusicVolume() * MasterVolume;

        private List<EventInstance> ListEvents;
    
        /// <summary>
        /// Instance unique de AudioManager accessible globalement
        /// </summary>
        public static AudioManager Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Tia tout chier debilus retire le component AudioManager de tous les objets et ne le met que sur un seul objet");
            }
            Instance = this;
        
            ListEvents = new List<EventInstance>();
        }
   

        /// <summary>
        /// Joue un son une seule fois à une position spécifique
        /// </summary>
        /// <param name="soundReference">Référence de l'événement sonore FMOD</param>
        /// <param name="position">Position 3D où jouer le son</param>
        public void PlayOneShot(EventReference soundReference, Vector3 position)
        {
            RuntimeManager.PlayOneShot(soundReference, position);
        }

        /// <summary>
        /// Crée une instance d'un événement sonore pouvant être contrôlée
        /// </summary>
        /// <param name="soundReference">Référence de l'événement sonore FMOD</param>
        /// <returns>Une instance d'événement FMOD contrôlable</returns>
        public EventInstance CreateEventInstance(EventReference soundReference)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(soundReference);
            ListEvents.Add(eventInstance);
            return eventInstance;
        }

        /// <summary>
        /// Configure les attributs 3D d'un événement sonore
        /// </summary>
        /// <param name="eventInstance">L'instance d'événement à configurer</param>
        /// <param name="position">Position 3D du son</param>
        /// <param name="velocity">Vitesse de l'émetteur (pour l'effet Doppler)</param>
        public void Set3DAttributes(EventInstance eventInstance, Vector3 position, Vector3 velocity = default)
        {
            FMOD.ATTRIBUTES_3D attributes = new FMOD.ATTRIBUTES_3D();
            attributes.position = new FMOD.VECTOR { x = position.x, y = position.y, z = position.z };
            attributes.velocity = new FMOD.VECTOR { x = velocity.x, y = velocity.y, z = velocity.z };
            eventInstance.set3DAttributes(attributes);
        }
    
        /// <summary>
        /// Configure les attributs 3D d'un événement sonore à partir d'un Transform
        /// </summary>
        /// <param name="eventInstance">L'instance d'événement à configurer</param>
        /// <param name="transform">Le Transform à partir duquel extraire la position</param>
        /// <param name="velocity">Vitesse de l'émetteur (pour l'effet Doppler)</param>
        public void Set3DAttributesFromTransform(EventInstance eventInstance, Transform transform, Vector3 velocity = default)
        {
            FMOD.ATTRIBUTES_3D attributes = RuntimeUtils.To3DAttributes(transform.position);
            attributes.velocity = new FMOD.VECTOR { x = velocity.x, y = velocity.y, z = velocity.z };
            eventInstance.set3DAttributes(attributes);
        }

        /// <summary>
        /// Configure les attributs 3D d'un événement sonore à partir d'un GameObject
        /// </summary>
        /// <param name="eventInstance">L'instance d'événement à configurer</param>
        /// <param name="gameObject">Le GameObject dont la position sera utilisée</param>
        /// <param name="velocity">Vitesse de l'émetteur (pour l'effet Doppler)</param>
        public void Set3DAttributesFromGameObject(EventInstance eventInstance, GameObject gameObject, Vector3 velocity = default)
        {
            eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.transform.position));
        }
    
        /// <summary>
        /// Arrête et libère toutes les instances d'événements sonores gérées par l'AudioManager.
        /// </summary>
        public void Cleanup()
        {
            foreach (var eventInstance in ListEvents)
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }
        }
    
        /// <summary>
        /// Destruction de l'AudioManager, nettoie les événements en cours.
        /// </summary>
        private void OnDestroy()
        {
            Cleanup();
        }
    
        /// <summary>
        /// Applique le volume approprié à chaque événement selon sa catégorie.
        /// </summary>
        public void ApplyVolumesToAllEvents()
        {
            foreach (var eventInstance in ListEvents)
            {
                string path = "";
                eventInstance.getDescription(out var desc);
                if (desc.isValid()) desc.getPath(out path);
            
                if (path.Contains("music", System.StringComparison.OrdinalIgnoreCase))
                    eventInstance.setVolume(MusicVolume);
                else
                    eventInstance.setVolume(SFXVolume);
            }
        }
    }
}