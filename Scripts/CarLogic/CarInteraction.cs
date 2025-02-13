using System;
using Interactable;
using Logic;
using Manager;
using ScriptableObjects;
using UnityEngine;

namespace CarLogic
{
    public class CarInteraction : InteractableObject
    {
        [SerializeField] private PlayerTransformValueSO _playerTransformValueSO;
        [SerializeField] private PlayerWeaponSettingsSO _playerCurrentWeaponSettingsSO;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Transform _cameraTarget;
        
        [Header("Exit Settings")]
        [SerializeField] private LayerMask _whatToIgnoreForExit;
        [SerializeField] private Transform[] _exitPoints;
        [SerializeField] private float _exitCheckRadius;
        
        private CarController _carController;
        private CarHealthController _carHealthController;
        
        private void Start()
        {
            _carController = GetComponent<CarController>();
            _carHealthController = GetComponent<CarHealthController>();

            foreach (var point in _exitPoints)
            {
                point.GetComponent<MeshRenderer>().enabled = false;
                //point.GetComponent<SphereCollider>().enabled = false;
            }
        }
        
        internal override void Interaction()
        {
            base.Interaction();
            
            GetInToTheCar();
        }

        private void GetInToTheCar()
        {
            ControlsManager.instance.SwitchToCarControls();
            
            _carHealthController.UpdateCarsHealthUI();
            
            _carController.ActivateCar(true);
            
            EventManager.PlayerEvents.PlayerScaleToZeroForCar?.Invoke(transform);
            EventManager.PlayerEvents.PlayerColliderFalse?.Invoke();
            EventManager.PlayerEvents.PlayerAimLaserFalse?.Invoke();
        }
        
        public void GetOutOfTheCar()
        {
            if (_carController._carActive == false)
                return;
            
            _carController.ActivateCar(false);
            
            ControlsManager.instance.SwitchToCharacterControls();
            _playerTransform.position = GetExitPoint();
            
            EventManager.PlayerEvents.PlayerScaleToDefault?.Invoke();
            EventManager.PlayerEvents.PlayerColliderTrue?.Invoke();
            EventManager.PlayerEvents.PlayerAimLaserTrue?.Invoke();
            
            // 2 kameraya çıkar Player ve Araba
            _cameraTarget.position = _playerTransformValueSO.PlayerTransform;
        }
        
        private Vector3 GetExitPoint()
        {
            for (int i = 0; i < _exitPoints.Length; i++)
            {
                if (IsExitClear(_exitPoints[i].position))
                {
                    return _exitPoints[i].position;
                }
            }
            
            return _exitPoints[0].position;
        }
        
        private bool IsExitClear(Vector3 point)
        {
            Collider[] colliders = Physics.OverlapSphere(point, _exitCheckRadius, ~_whatToIgnoreForExit);
            return colliders.Length == 0;
        }

        private void OnDrawGizmos()
        {
            if (_exitPoints.Length > 0)
            {
                foreach (var exitPoint in _exitPoints)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(exitPoint.position, _exitCheckRadius);
                }
            }
        }
    }
}