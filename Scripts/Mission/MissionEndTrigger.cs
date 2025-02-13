using Manager;
using ScriptableObjects;
using UnityEngine;

namespace MissionLogic
{
    public class MissionEndTrigger : MonoBehaviour
    {
        [SerializeField] private PlayerTransformValueSO _playerTransformValueSO;
        
        private bool _isMissionComplete;
        
        private void Update()
        {
            SuccessFromTransform();
        }
        
        private void SuccessFromTransform()
        {
            if (_isMissionComplete)
                return;
                
            if (Vector3.Distance(transform.position, _playerTransformValueSO.PlayerTransform) < 1 && MissionManager.instance.IsMissionComplete())
            {
                _isMissionComplete = true;
                EventManager.GameEvents.LevelCompleted?.Invoke();
            }
        }
    }
}