using ScriptableObjects;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    
    public class PlayerMovementData
    {
        [SerializeField] private PlayerMovementValueSO _playerMovementValueSO;
        
        private CharacterController _characterController;
        private PlayerControllerData _playerControllerData;
        private Animator _animator;
        
        public CharacterController CharacterController
        {
            get { return _characterController; }
            set { _characterController = value; }
        }
        
        public Animator Animator
        {
            get { return _animator; }
            set { _animator = value; }
        }
        
        public PlayerMovementValueSO PlayerMovementValueSO
        {
            get { return _playerMovementValueSO; }
            set { _playerMovementValueSO = value; }
        }
        
        public PlayerControllerData PlayerControllerData
        {
            get { return _playerControllerData; }
            set { _playerControllerData = value; }
        }
    }
}