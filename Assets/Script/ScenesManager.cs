using BHR;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BHR
{
    public class ScenesManager : MonoBehaviour
    {
        public static ScenesManager Instance;

        [SerializeField]
        private ScenesDataSO ScenesData;
        [SerializeField]
        private GameObject _canvasPrefab;

        private void Awake()
        {
            // Creates instance
            if(Instance == null)
                Instance = this;

            // Clean up existing canvas
            if(GameObject.Find("Canvas"))
            {
                Debug.LogWarning("Destroying present canvas");
                Destroy(GameObject.Find("Canvas"));
                ModuleManager.Instance = null;
            }
            var canvas = Instantiate(_canvasPrefab);
            DontDestroyOnLoad(canvas);

            ModuleManager.Instance.Init();
            string firstScene = SceneManager.GetActiveScene().name;
            if (firstScene == ScenesData.MainMenuScene)
            {
                ModuleManager.Instance.MainTitleModule(true);
            }
            else if (ScenesData.LevelsScene.Contains(firstScene))
            {
                // Load a level, HUD etc
            }
            else if(firstScene == ScenesData.TestScene)
            {
                // Debug scene
            }
        }

        public void ChangeScene(string scene)
        {
            Debug.Log(scene);
            if(ScenesData.LevelsScene.Contains(scene) || scene == ScenesData.MainMenuScene || scene == ScenesData.TestScene)
                SceneManager.LoadScene(scene);
            else if(scene == "Quit")
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                Application.Quit();
            }
        }
    }
}
