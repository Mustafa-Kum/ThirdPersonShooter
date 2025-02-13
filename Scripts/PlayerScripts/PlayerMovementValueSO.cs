using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerMovementValueSO", menuName = "PlayerMovementValueSO", order = 0)]
    public class PlayerMovementValueSO : ScriptableObject
    {
        [HideInInspector] public Vector3 _moveDirection;
        
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _runSpeed;
        [SerializeField] private float _defaultSpeed;
        
        [SerializeField] private Vector2 _moveInput;
        private float _gravityScale = 9.81f;
        private float _verticalVelocity;
        private bool _isRunning;
        private bool _isRolling;
        
        public float MoveSpeed
        {
            get { return _moveSpeed; }
            set { _moveSpeed = value; }
        }
        
        public float RunSpeed
        {
            get { return _runSpeed; }
            set { _runSpeed = value; }
        }
        
        public float DefaultSpeed
        {
            get { return _defaultSpeed; }
            set { _defaultSpeed = value; }
        }
        
        public Vector2 MoveInput
        {
            get { return _moveInput; }
            set { _moveInput = value; }
        }
        
        public float GravityScale
        {
            get { return _gravityScale; }
            set { _gravityScale = value; }
        }
        
        public float VerticalVelocity
        {
            get { return _verticalVelocity; }
            set { _verticalVelocity = value; }
        }
        
        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }
        
        public bool IsRolling
        {
            get { return _isRolling; }
            set { _isRolling = value; }
        }
    }
}