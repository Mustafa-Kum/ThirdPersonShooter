using ScriptableObjects;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Data
{
    [System.Serializable]
    
    public class PlayerWeaponReloadData
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerRigSettingsSO _playerRigSettingsSO;
        [SerializeField] private PlayerWeaponSettingsSO _currentWeaponSettingsSO;
        [SerializeField] private PlayerWeaponIndexSO _playerWeaponIndexSO;
        [SerializeField] private Rig _rig;
        [SerializeField] private float _rigIncreaseSpeed;
        [SerializeField] private AudioSource[] _weaponReloadSound;
        
        public Animator Animator
        {
            get { return _animator; }
            set { _animator = value; }
        }
        
        public Rig Rig
        {
            get { return _rig; }
            set { _rig = value; }
        }
        
        public float RigIncreaseSpeed
        {
            get { return _rigIncreaseSpeed; }
            set { _rigIncreaseSpeed = value; }
        }
        
        public PlayerRigSettingsSO PlayerRigSettingsSO
        {
            get { return _playerRigSettingsSO; }
            set { _playerRigSettingsSO = value; }
        }
        
        public PlayerWeaponIndexSO PlayerWeaponIndexSO
        {
            get { return _playerWeaponIndexSO; }
            set { _playerWeaponIndexSO = value; }
        }

        public PlayerWeaponSettingsSO CurrentWeaponSettingsSO
        {
            get { return _currentWeaponSettingsSO; }
            set { _currentWeaponSettingsSO = value; }
        }
        
        public AudioSource[] WeaponReloadSound
        {
            get { return _weaponReloadSound; }
            set { _weaponReloadSound = value; }
        }
    }
}