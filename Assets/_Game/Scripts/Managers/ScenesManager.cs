using BHR;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
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

        public UnityEvent<string> OnSceneSuccessfulyLoaded;

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
#if UNITY_EDITOR
            if(_startSceneData == null)
            {
                LevelDataSO debugScene = ScriptableObject.CreateInstance<LevelDataSO>();
                debugScene.SceneName = SceneManager.GetActiveScene().name;
                GameManager.Instance.SelectedLevel = debugScene;
                _startSceneData = debugScene;
            }
#endif
            CurrentSceneData = _startSceneData;
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            LoadDefaultSceneData(CurrentSceneData);
        }

        public void ChangeScene(SceneDataSO sceneData, bool withEndTransition = true)
        {
            CurrentSceneData = sceneData;

            if(SceneManager.GetActiveScene().name != sceneData.SceneName)
            {
                Debug.Log($"Changing scene from {SceneManager.GetActiveScene().name} to {sceneData.SceneName}");
                LoadSceneWithTransition(sceneData.SceneName, withEndTransition);
            }
        }

        public void ReloadScene(bool withTransition = false)
        {
            if(CurrentSceneData != null)
            {
                Debug.Log($"Reloading {CurrentSceneData.SceneName} scene");
                if (withTransition)
                    LoadSceneWithTransition(CurrentSceneData.SceneName, false);
                else
                    LoadScene(CurrentSceneData.SceneName);
            }
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
            OnSceneSuccessfulyLoaded?.Invoke(scene.name);
        }

        private void LoadDefaultSceneData(SceneDataSO sceneData)
        {
            GameObject moduleToLoad = null;
            if (sceneData is LevelDataSO || sceneData == null)
            {
                GameManager.Instance.IsPlaying = false;
                GameManager.Instance.SaveSelectedLevel(sceneData as LevelDataSO);
                moduleToLoad = ModuleManager.Instance.GetModule(ModuleType.PLAYER_SELECTION);
            }
            else if (sceneData.SceneName == "MainMenu")
            {
                moduleToLoad = ModuleManager.Instance.GetModule(ModuleType.MAIN_TITLE);
                PlayersInputManager.Instance.CanConnect = true;
            }
            ModuleManager.Instance.ProcessModuleState(moduleToLoad, true);
        }

       private void LoadSceneWithTransition(string sceneName, bool withEndTransition = true) => StartCoroutine(LoadSceneAsync(sceneName, withEndTransition));
       private void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
        
       IEnumerator LoadSceneAsync(string sceneName, bool withEndTransition = true)
        {
            // Start transition animation
            ModuleManager.Instance.LaunchTransitionAnimation(true);
            yield return new WaitForSeconds(ModuleManager.Instance.DefaultTransitionDuration);

            // Scene loading
            AsyncOperation loading = SceneManager.LoadSceneAsync(sceneName);
            while (!loading.isDone) { yield return null; }

            // End transition animation
            if(withEndTransition)
            {
                ModuleManager.Instance.LaunchTransitionAnimation(false);
                yield return new WaitForSeconds(ModuleManager.Instance.DefaultTransitionDuration);
            }
            else
            {
                ModuleManager.Instance.SceneTransitionHasFinished = true;
                ModuleManager.Instance.CheckStartAnimationLaunchConditions();
            }
        }
    }
}
