using System;
using Manager;
using ScriptableObjects;
using UnityEngine;

namespace MissionLogic
{
    public class MissionObject_Key : MonoBehaviour
    {
        public static event Action OnKeyPickedUp;
        
        [SerializeField] private PlayerTransformValueSO _playerTransformValueSO;
        
        private void Update()
        {
            PickedUpKey();
        }
        
        private void PickedUpKey()
        {
            if (Vector3.Distance(transform.position, _playerTransformValueSO.PlayerTransform) < 1)
            {
                Debug.Log("Key Picked Up");
                OnKeyPickedUp?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}