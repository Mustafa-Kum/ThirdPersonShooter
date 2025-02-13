using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    
    public class PlayerWeaponEquipAndPickUpData
    {
        [SerializeField] private List<PlayerWeaponSettingsSO> _playerWeaponSlotSO = new List<PlayerWeaponSettingsSO>();
        [SerializeField] private PlayerWeaponSettingsSO _currentWeaponSettingsSO;
        [SerializeField] private PlayerWeaponIndexSO _playerWeaponIndexSO;
        
        private PlayerControllerData _playerControllerData;
        private int _maxSlotAllowed = 5;
        
        public PlayerControllerData PlayerControllerData
        {
            get { return _playerControllerData; }
            set { _playerControllerData = value; }
        }
        
        public List<PlayerWeaponSettingsSO> PlayerWeaponSlotSO
        {
            get { return _playerWeaponSlotSO; }
            set { _playerWeaponSlotSO = value; }
        }
        
        public PlayerWeaponSettingsSO CurrentWeaponSettingsSO
        {
            get { return _currentWeaponSettingsSO; }
            set { _currentWeaponSettingsSO = value; }
        }
        
        public PlayerWeaponIndexSO PlayerWeaponIndexSO
        {
            get { return _playerWeaponIndexSO; }
            set { _playerWeaponIndexSO = value; }
        }
        
        public int MaxSlotAllowed
        {
            get { return _maxSlotAllowed; }
        }
    }
}