using System.Collections;
using UnityEngine;
using Cinemachine;
using Manager;
using ScriptableObjects;

namespace PlayerScripts
{
    public class PlayerFallToGround : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private PlayerWeaponSettingsSO _playerCurrentWeaponSettingsSO;
        [SerializeField] private PlayerMovementValueSO _playerMovementValueSO;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                EventManager.PlayerEvents.PlayerFallingToGround?.Invoke();
            }
        }

        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerFallingToGround += PlayerFallingToGround;
            EventManager.PlayerEvents.PlayerGetUpFromGround += PlayerGetUpFromGround;
        }

        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerFallingToGround -= PlayerFallingToGround;
            EventManager.PlayerEvents.PlayerGetUpFromGround -= PlayerGetUpFromGround;
        }

        private void PlayerFallingToGround()
        {
            // İlgili event'leri tetikleyebilirsiniz, örneğin:
            EventManager.PlayerEvents.PlayerFOVZoomRoutine?.Invoke(105f, 0.2f, _virtualCamera);
            EventManager.PlayerEvents.PlayerScopedAnimation?.Invoke(false);
            EventManager.PlayerEvents.PlayerControlBool?.Invoke(false);
            _animator.SetTrigger("FallToGround");
            _playerCurrentWeaponSettingsSO.IsWeaponScopped = false; 
            _playerMovementValueSO.IsRunning = false;
        }

        private void PlayerGetUpFromGround()
        {
            EventManager.PlayerEvents.PlayerFOVZoomRoutine?.Invoke(75f, 0.2f, _virtualCamera);
            EventManager.PlayerEvents.PlayerControlBool?.Invoke(true);
        }
    }
}
