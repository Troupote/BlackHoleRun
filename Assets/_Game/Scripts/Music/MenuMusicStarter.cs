using UnityEngine;
using BHR;
using _Game.Scripts.Music;
using FMOD.Studio;
using UnityEngine.SceneManagement;

public class MenuMusicStarter : MonoBehaviour
{
    private EventInstance _menuMusicInstance;
    private string _menuSceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Start menu music and remember scene
        _menuSceneName = SceneManager.GetActiveScene().name;
        _menuMusicInstance = AudioManager.Instance.CreateEventInstance(FmodEventsCreator.instance.MusicGeneralMenu);
        _menuMusicInstance.setVolume(AudioManager.Instance.MusicVolume);
        _menuMusicInstance.start();
        // Listen for scene changes to stop music
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != _menuSceneName && _menuMusicInstance.isValid())
        {
            _menuMusicInstance.stop(STOP_MODE.ALLOWFADEOUT);
            _menuMusicInstance.release();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
