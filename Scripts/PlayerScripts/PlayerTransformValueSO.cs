using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerTransformValueSO", menuName = "PlayerTransformValueSO", order = 0)]
    public class PlayerTransformValueSO : ScriptableObject
    {
        [SerializeField] private Vector3 _playerTransform;
        
        public Vector3 PlayerTransform
        {
            get => _playerTransform;
            set => _playerTransform = value;
        }
    }
}