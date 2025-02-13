using System;
using UILogic;
using UnityEngine;

namespace Manager
{
    public class GameOverEvents : MonoBehaviour
    {
        private void OnEnable()
        {
            EventManager.GameEvents.GameOver += ShowGameOverUI;
        }
        
        private void OnDisable()
        {
            EventManager.GameEvents.GameOver -= ShowGameOverUI;
        }
        
        private void ShowGameOverUI()
        {
            TimeManager.instance.SlowMotion(2f);
            UI.instance.ShowGameOverUI();
        }
    }
}