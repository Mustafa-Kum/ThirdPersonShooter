using Cinemachine;
using Lean.Pool;
using Manager;
using ScriptableObjects;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerRoll : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private PlayerWeaponSettingsSO _playerCurrentWeaponSettingsSO;
        [SerializeField] private PlayerMovementValueSO _playerMovementValueSO;
        [SerializeField] private GameObject _rollParticle;
        [SerializeField] private float _rollSpeed;
        [SerializeField] private float _rollCooldown;

        private Animator _animator;
        private bool _isRolling;
        private bool _canRoll = true;
        private float _currentCooldown;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerRollingStart += RollingStartControl;
            EventManager.PlayerEvents.PlayerRollingEnd += RollingEndControl;
            EventManager.PlayerEvents.PlayerRollParticle += RollParticle;
        }

        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerRollingStart -= RollingStartControl;
            EventManager.PlayerEvents.PlayerRollingEnd -= RollingEndControl;
            EventManager.PlayerEvents.PlayerRollParticle -= RollParticle;
        }

        private void Update()
        {
            if (_isRolling)
            {
                MoveForward();
            }

            HandleCooldown();
        }

        private void HandleCooldown()
        {
            if (!_canRoll)
            {
                _currentCooldown -= Time.deltaTime;
                
                if (_currentCooldown <= 0)
                {
                    _canRoll = true;
                }
            }
        }

        private void MoveForward()
        {
            transform.Translate(Vector3.forward * (_rollSpeed * Time.deltaTime));
        }

        private void RollingStartControl(bool isRolling)
        {
            if (!_canRoll) return;
            
            _isRolling = isRolling;
            _animator.SetBool("Roll", isRolling);
            _playerMovementValueSO.IsRolling = true;
            EventManager.PlayerEvents.PlayerCanTakeDamage?.Invoke(false);
            EventManager.PlayerEvents.PlayerScopedAnimation?.Invoke(false);
            EventManager.PlayerEvents.PlayerWeaponCanFire?.Invoke(false);
            EventManager.PlayerEvents.PlayerRollingCameraEffect?.Invoke();
            
            _playerMovementValueSO.IsRunning = false;
            _playerCurrentWeaponSettingsSO.IsWeaponScopped = false; 
            
            StartCooldown();
        }
        
        private void RollingEndControl(bool isRolling)
        {
            _isRolling = isRolling;
            _animator.SetBool("Roll", isRolling);
            _playerMovementValueSO.IsRolling = false;
            EventManager.PlayerEvents.PlayerCanTakeDamage?.Invoke(true);
            EventManager.PlayerEvents.PlayerWeaponCanFire?.Invoke(true);
        }

        private void StartCooldown()
        {
            _canRoll = false;
            _currentCooldown = _rollCooldown;
        }

        private void RollParticle()
        {
            GameObject rollParticle = LeanPool.Spawn(_rollParticle, transform.position, Quaternion.identity);
            LeanPool.Despawn(rollParticle, 3f);
        }
    }
}
