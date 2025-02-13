using UnityEngine;

namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        
        private void Awake()
        {
            instance = this;
            
            EventManager.GameEvents.GameStart?.Invoke();
        }
    }
}