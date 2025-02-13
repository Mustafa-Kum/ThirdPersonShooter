using Controller;
using Logic;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    [System.Serializable]
    
    public class PlayerAimData
    {
        [Header("Scriptable Objects and Components")]
        [SerializeField] private PlayerAimSO playerAimSo;
        [SerializeField] private PlayerWeaponSettingsSO _playerCurrentWeaponSettingsSO;
        
        [Header("Camera and Aiming")]
        [SerializeField] private Transform _cameraTarget;
        
        private PlayerControllerData _playerControllerData;

        public Transform CameraTarget
        {
            get { return _cameraTarget; }
            set { _cameraTarget = value; }
        }
        
        public PlayerAimSO PlayerAimSo
        {
            get { return playerAimSo; }
            set { playerAimSo = value; }
        }
        
        public PlayerWeaponSettingsSO PlayerCurrentWeaponSettingsSO
        {
            get { return _playerCurrentWeaponSettingsSO; }
            set { _playerCurrentWeaponSettingsSO = value; }
        }
        
        public PlayerControllerData PlayerControllerData
        {
            get { return _playerControllerData; }
            set { _playerControllerData = value; }
        }
    }
}