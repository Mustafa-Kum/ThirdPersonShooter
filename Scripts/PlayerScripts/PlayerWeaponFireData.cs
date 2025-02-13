    using ScriptableObjects;
    using UnityEngine;

    namespace Data
    {
        [System.Serializable]
        
        public class PlayerWeaponFireData
        {
            [SerializeField] private PlayerRigSettingsSO _playerRigSettingsSO;
            [SerializeField] private PlayerBulletSettingsSO _playerBulletSettingsSO;
            [SerializeField] private PlayerWeaponIndexSO _playerWeaponIndexSO;
            [SerializeField] private PlayerWeaponSettingsSO _currentWeaponSettingsSO;
            [SerializeField] private PlayerAimSO _playerAimSO;
            [SerializeField] private Transform _weaponHolder;
            [SerializeField] private Transform _aimTransform;
            [SerializeField] private ParticleSystem[] _muzzleFlash;
            [SerializeField] private Transform[] _gunPoint;
            
            private Animator _animator;
            
            public PlayerRigSettingsSO PlayerRigSettingsSO
            {
                get { return _playerRigSettingsSO; }
                set { _playerRigSettingsSO = value; }
            }
            
            public PlayerBulletSettingsSO PlayerBulletSettingsSO
            {
                get { return _playerBulletSettingsSO; }
                set { _playerBulletSettingsSO = value; }
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
            
            public PlayerAimSO PlayerAimSo
            {
                get { return _playerAimSO; }
                set { _playerAimSO = value; }
            }
            
            public Transform[] GunPoint
            {
                get { return _gunPoint; }
                set { _gunPoint = value; }
            }
            
            public Transform WeaponHolder
            {
                get { return _weaponHolder; }
                set { _weaponHolder = value; }
            }
            
            public Transform AimTransform
            {
                get { return _aimTransform; }
                set { _aimTransform = value; }
            }
            
            public Animator Animator
            {
                get { return _animator; }
                set { _animator = value; }
            }
            
            public ParticleSystem[] MuzzleFlash
            {
                get { return _muzzleFlash; }
                set { _muzzleFlash = value; }
            }
        }
    }