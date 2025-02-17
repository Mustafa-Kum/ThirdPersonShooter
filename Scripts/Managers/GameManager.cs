using UnityEngine;

namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        
        private void Awake()
        {
            instance = this;
        
        }

        private void Start()
        {
            // Start metodunda event tetikleyerek, diğer script'lerin abone olma şansını artırıyoruz.
            EventManager.GameEvents.GameStart?.Invoke();
        }
    }
}