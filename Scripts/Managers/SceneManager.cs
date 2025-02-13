using System.Collections;
using Interface;
using UnityEngine;

namespace Manager
{
    public class SceneManager : MonoBehaviour, ISceneLoader
    {
        [SerializeField] private float delay = 1.0f;

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneWithDelay(sceneName));
        }

        public void RestartScene()
        {
            StartCoroutine(LoadSceneWithDelay(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));
        }

        public void ToMainMenu(string sceneName)
        {
            StartCoroutine(LoadSceneWithDelay(sceneName));
        }

        public void QuitGame()
        {
            StartCoroutine(QuitGameWithDelay());
        }

        private IEnumerator LoadSceneWithDelay(string sceneName)
        {
            TimeManager.instance.ResumeTime();
            EventManager.UIEvents.UIFadeOut?.Invoke();
            yield return new WaitForSeconds(delay);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        private IEnumerator QuitGameWithDelay()
        {
            EventManager.UIEvents.UIFadeOut?.Invoke();
            yield return new WaitForSeconds(delay);
            Application.Quit();
        }
    }
}