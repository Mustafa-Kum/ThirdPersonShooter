using System.Collections.Generic;
using Manager;
using ScriptableObjects;
using UnityEngine;

namespace Logic
{
    public class HealthBoxPickedUpLogic : MonoBehaviour
    {
        [SerializeField] private HealthData _healthBox;
        [SerializeField] private PlayerHealthDataSO _playerHealthDataSO;

        private void OnEnable()
        {
            SubscribeEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            EventManager.PlayerEvents.PlayerHealthPickedUp += HealthPickedUp;
        }

        private void UnsubscribeEvents()
        {
            EventManager.PlayerEvents.PlayerHealthPickedUp -= HealthPickedUp;
        }

        private void HealthPickedUp()
        {
            _playerHealthDataSO._currentHealthAmount += _healthBox._healthAmount;
        }
    }

    [System.Serializable]
    public struct HealthData
    {
        public int _healthAmount;
    }
}