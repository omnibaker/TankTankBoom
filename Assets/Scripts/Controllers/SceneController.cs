using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;

namespace Sumfulla.TankTankBoom
{
    /// <summary>
    /// Control loading/unloading of all scenes in the game; all scene are Additive to the global Main scene
    /// </summary>
    public class SceneController : Singleton<SceneController>
    {
        public Scene Scene_Current { set; get; } 
        public Scene Scene_Loading { set; get; }

        private static bool _isLoading;
        private VisualElement _sceneFade; 
        private Coroutine _fading;


        private void Awake()
        {
            CreateInstance(this, gameObject);
        }

        private void OnEnable()
        {
            // Listen for Unity's callback after any scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Prepare the UI root and fade layer
            UIDocument uid = GetComponent<UIDocument>();
            VisualElement root = GameUtils.UITK.ConfigureRoot(uid, GameRef.UIRef.UXML_FOREGROUND);

            _sceneFade = root.Q(GameRef.UIRef.FOREGROUND_FADE);
            GameData.Foreground = root.Q(GameRef.UIRef.FOREGROUND_MESSAGES);

            // Start with fade screen hidden
            EnableSegueScreen(false);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            // If we're booting inside MAIN, immediately load the MENU
            if (Scene_Loading.name == GameRef.Scenes.MAIN)
            {
                GoToScene(GameRef.Scenes.MENU);
            }
        }

        /// <summary>
        /// Initiates a scene transition, if one isn't already happening
        /// </summary>
        public void GoToScene(string sceneName)
        {
            if (_isLoading)
            {
                GameLog.Say($"Ignoring GoToScene('{sceneName}') � already loading.");
                return;
            }

            if (Scene_Current.IsValid() && Scene_Current.name == sceneName)
            {
                GameLog.Say($"Ignoring GoToScene('{sceneName}') � already in this scene.");
                return;
            }

            _isLoading = true;
            StartCoroutine(FadeAndLoad(sceneName));
        }

        /// <summary>
        /// Plays the fade-out animation, waits for it, then loads a new additive scene
        /// </summary>
        private IEnumerator FadeAndLoad(string sceneName)
        {
            // Trigger UI fade-to-black
            EnableSegueScreen(true);
            yield return new WaitForSeconds(GameRef.Time.SCENE_FADE);

            // Load the next scene additively
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Called from Unity when scene is loaded
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Only process additive loads (ignore initial Main scene)
            if (mode != LoadSceneMode.Additive) return;

            Scene_Loading = scene;

            // Make the newly loaded scene the active one
            SceneManager.SetActiveScene(scene);

            // Disable and unload the previous scene if one existed
            if (Scene_Current.IsValid())
            {
                DisableOldScene();
            }

            // Update current scene
            Scene_Current = Scene_Loading;

            // Turn off loading flag
            _isLoading = false;
            
            // Start fade in transition
            StartCoroutine(Unfade());

        }

        /// <summary>
        /// Waits for the fade-in delay, then removes the fade overlay
        /// </summary>
        private IEnumerator Unfade()
        {
            yield return new WaitForSeconds(2f * GameRef.Time.SCENE_FADE);
            EnableSegueScreen(false);
        }

        /// <summary>
        /// Deactivates and unloads the previously active scene to avoid interference
        /// </summary>
        private void DisableOldScene()
        {
            if (!Scene_Current.IsValid()) return;

            // Disable all root objects
            foreach (GameObject obj in Scene_Current.GetRootGameObjects())
            {
                obj.SetActive(false);
            }

            // Start async unload
            SceneManager.UnloadSceneAsync(Scene_Current);
        }

        /// <summary>
        /// Sets the fade overlay opacity to trigger its animation
        /// </summary>
        public void EnableSegueScreen(bool fadeOut)
        {
            _sceneFade.style.opacity = fadeOut ? 1f : 0f;
        }

        public void FadeOutIn(Action action)
        {
            if(_fading != null)
            {
                StopCoroutine(_fading);
            }

            _fading = StartCoroutine(RunFadeOutIn(action));
        }

        public IEnumerator RunFadeOutIn(Action action)
        {
            EnableSegueScreen(true);
            yield return new WaitForSeconds(2f * GameRef.Time.SCENE_FADE + 1f);
            action?.Invoke();
            yield return new WaitForSeconds(2f * GameRef.Time.SCENE_FADE + 1f);
            EnableSegueScreen(false);
        }
    }
}