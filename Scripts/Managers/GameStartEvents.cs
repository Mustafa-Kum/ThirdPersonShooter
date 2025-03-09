using MissionLogic;
using ScriptableObjects;
using UnityEngine;

namespace Manager
{
    public class GameStartEvents : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponSettingsSO _playerCurrentWeaponSettingsSO;
        [SerializeField] private Mission _firstMission;

        private void OnEnable()
        {
            EventManager.GameEvents.GameStart += OnGameStart;
        }

        private void OnDisable()
        {
            EventManager.GameEvents.GameStart -= OnGameStart;
        }

        private void OnGameStart()
        {
            ShootTimeReset();
            //LevelGeneration();
            EventManager.EnemySpawnEvents.EnemySpawn?.Invoke();
            EventManager.PlayerEvents.PlayerWeaponSwap?.Invoke(0);
        }
        
        private void ShootTimeReset()
        {
            _playerCurrentWeaponSettingsSO.ResetShootTime();
        }

        private void LevelGeneration()
        {
            EventManager.GameEvents.LevelGeneration?.Invoke();
        }
    }
}