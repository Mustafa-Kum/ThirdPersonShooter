using Logic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Data
{
    [System.Serializable]
    public class PlayerWeaponSwapperData
    {
        [SerializeField] private PlayerWeaponIndexSO _playerWeaponIndexSO;
        [SerializeField] private PlayerRigSettingsSO _playerRigSettingsSO; 
        [SerializeField] private Transform _leftHandIKTargetTransform;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rig _rig;
        [SerializeField] private Transform[] _gunTransforms;
        [SerializeField] private Transform[] _leftHandTargetTransforms;
        [SerializeField] private AudioSource[] _weaponSwapSound;

        public Transform[] GunTransforms
        {
            get { return _gunTransforms; }
            set { _gunTransforms = value; }
        }
        
        public Transform[] LeftHandTargetTransforms
        {
            get { return _leftHandTargetTransforms; }
            set { _leftHandTargetTransforms = value; }
        }
        
        public Transform LeftHandIKTargetTransform
        {
            get { return _leftHandIKTargetTransform; }
            set { _leftHandIKTargetTransform = value; }
        }
        
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
        
        public PlayerWeaponIndexSO PlayerWeaponIndexSO
        {
            get { return _playerWeaponIndexSO; }
            set { _playerWeaponIndexSO = value; }
        }
        
        public PlayerRigSettingsSO PlayerRigSettingsSO
        {
            get { return _playerRigSettingsSO; }
            set { _playerRigSettingsSO = value; }
        }
        
        public AudioSource[] WeaponSwapSound
        {
            get { return _weaponSwapSound; }
            set { _weaponSwapSound = value; }
        }
    }
}