using UnityEngine;

namespace Interface
{
    public interface ISceneLoader
    {
        void LoadScene(string sceneName);
        void RestartScene();
        void ToMainMenu(string sceneName);
        void QuitGame();
    }
}