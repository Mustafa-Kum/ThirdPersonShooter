using UnityEngine;

namespace Data
{
    public class PlayerControllerData : MonoBehaviour
    {
        public PlayerController _playerController;

        private void Awake()
        {
            _playerController = new PlayerController();
        }
        
        private void OnEnable()
        {
            _playerController.Enable();
        }
    
        private void OnDisable()
        {
            _playerController.Disable();
        }
    }
}