using BHR;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BHR
{
    public class ScenesManager : ManagerSingleton<ScenesManager>
    {
        [SerializeField, ValidateInput("ValidateStartSceneData", "Start scene must be the same as the current scene !")]
        private SceneDataSO _startSceneData;
        public SceneDataSO MenuScene;

        [ReadOnly]
        public SceneDataSO CurrentSceneData;
        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        #region Validate Inputs
#if UNITY_EDITOR
        private bool ValidateStartSceneData()
        {
            return _startSceneData?.SceneName == SceneManager.GetActiveScene().name || _startSceneData == null;
        }

#endif
        #endregion

        public override void Awake()
        {
            base.Awake();
            CurrentSceneData = _startSceneData;
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            LoadDefaultSceneData(CurrentSceneData);
        }

        public void ChangeScene(SceneDataSO sceneData)
        {
            Debug.Log($"Changing scene from {SceneManager.GetActiveScene().name} to {sceneData.SceneName}");
            CurrentSceneData = sceneData;

            if(SceneManager.GetActiveScene().name != sceneData.SceneName)
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
            
        }

        private void LoadDefaultSceneData(SceneDataSO sceneData)
        {
            GameObject moduleToLoad = null;
            if (sceneData is LevelDataSO || sceneData == null)
            {
                GameManager.Instance.IsPlaying = false;
                GameManager.Instance.SaveSelectedLevel(sceneData as LevelDataSO);
                moduleToLoad = ModuleManager.Instance.GetModule(ModuleManager.ModuleType.PLAYER_SELECTION);
            }
            else if (sceneData.SceneName == "MainMenu")
            {
                moduleToLoad = ModuleManager.Instance.GetModule(ModuleManager.ModuleType.MAIN_TITLE);
                PlayersInputManager.Instance.CanConnect = true;
            }
            ModuleManager.Instance.ProcessModuleState(moduleToLoad, true);
        }
    }
}
