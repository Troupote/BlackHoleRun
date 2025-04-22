using BHR;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BHR
{
    public class ScenesManager : MonoBehaviour
    {
        public static ScenesManager Instance;
        [SerializeField]
        private GameObject _canvasPrefab;

        [SerializeField, ValidateInput("ValidateStartSceneData", "Start scene must be the same as the current scene !")]
        private SceneDataSO _startSceneData;

        [ReadOnly]
        public SceneDataSO CurrentSceneData;

        #region Validate Inputs
#if UNITY_EDITOR
        private bool ValidateStartSceneData()
        {
            return _startSceneData?.SceneName == SceneManager.GetActiveScene().name;
        }

#endif
        #endregion

        private void Awake()
        {
            // Creates instance
            if (Instance != null)
                return;

            Instance = this;
            CurrentSceneData = _startSceneData;

            DontDestroyOnLoad(gameObject);

            // Set prior and unique canvas
            ResetCanvas(true);
            ModuleManager.Instance = null;
            var canvas = Instantiate(_canvasPrefab);
            Debug.Log("Creating unique Canvas");
            DontDestroyOnLoad(canvas);
            canvas.tag = "Untagged";

            ModuleManager.Instance.Init();
            EnableDefaultModule(CurrentSceneData);
        }

        private void ResetCanvas(bool itself = false)
        {
            if (GameObject.FindGameObjectsWithTag("Canvas").Length > 0)
            {
                Debug.LogWarning("Destroying present canvas");
                foreach (GameObject go in GameObject.FindGameObjectsWithTag("Canvas"))
                    if(!itself || go!=this.gameObject)
                        Destroy(go);
            }
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void ChangeScene(SceneDataSO sceneData)
        {
            Debug.Log($"Changing scene from {SceneManager.GetActiveScene().name} to {sceneData.SceneName}");
            SceneManager.LoadScene(sceneData.SceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResetCanvas();
        }

        private void EnableDefaultModule(SceneDataSO sceneData)
        {
            GameObject moduleToLoad = null;
            if (sceneData.SceneName == "MainMenu")
            {
                moduleToLoad = ModuleManager.Instance.MainMenuDefaultModule;
            }
            else if (sceneData.SceneName.Contains("Level"))
            {
                moduleToLoad = ModuleManager.Instance.LevelDefaultModule;
            }
            else if (sceneData.SceneName == "Test")
            {
                moduleToLoad = ModuleManager.Instance.TestDefaultModule;
            }
            ModuleManager.Instance.ProcessModuleState(moduleToLoad, true);
        }
    }
}
