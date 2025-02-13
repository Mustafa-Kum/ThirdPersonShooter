using ScriptableObjects;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    
    public class PlayerBulletLogicData
    {
        [SerializeField] private PlayerWeaponIndexSO _playerWeaponIndexSO;
        [SerializeField] private PlayerBulletSettingsSO _playerBulletSettingsSO;
        [SerializeField] private PlayerWeaponSettingsSO _playerWeaponSettingsSO;
        
        private Vector3 _startPosition;
        private float _gunRange;
        private Rigidbody _rigidbody;
        
        public PlayerWeaponIndexSO PlayerWeaponIndexSO => _playerWeaponIndexSO;
        public PlayerBulletSettingsSO PlayerBulletSettingsSO => _playerBulletSettingsSO;
        public PlayerWeaponSettingsSO PlayerWeaponSettingsSO => _playerWeaponSettingsSO;
        public Vector3 StartPosition
        {
            get => _startPosition;
            set => _startPosition = value;
        }
        
        public float GunRange
        {
            get => _gunRange;
            set => _gunRange = value;
        }
        
        public Rigidbody Rigidbody
        {
            get => _rigidbody;
            set => _rigidbody = value;
        }
    }
}