using System;
using Cinemachine;
using Logic;
using Manager;
using ScriptableObjects;
using UnityEngine;

namespace AnimationEvents
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        [SerializeField] private PlayerRigSettingsSO playerRigSettingsSO;
        [SerializeField] private AudioSource _playerWalkSound;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        
        private PlayerWeaponReloadLogic _playerWeaponReloadLogic;
        private Animator _animator;

        private void OnEnable()
        {
            SubscribeEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Awake()
        {
            InitializeComponents();
        }

        private void SubscribeEvents()
        {
            EventManager.PlayerEvents.PlayerScopedAnimation += PlayerScopeAnimation;
        }

        private void UnsubscribeEvents()
        {
            EventManager.PlayerEvents.PlayerScopedAnimation -= PlayerScopeAnimation;
        }

        private void InitializeComponents()
        {
            _playerWeaponReloadLogic = GetComponentInParent<PlayerWeaponReloadLogic>();
            _animator = GetComponent<Animator>();
        }

        public void OnReloadStart()
        {
            StartReloadProcess();
        }
        
        public void OnReloadComplete()
        {
            CompleteReloadProcess();
        }
        
        public void OnWeaponSwapComplete()
        {
            CompleteWeaponSwap();
        }
        
        public void OnWeaponSwapStart()
        {
            StartWeaponSwap();
        }
        
        public void PlayerLeftFootStepWalkSound()
        {
            PlayFootstepWalkSound(true);
        }
        
        public void PlayerRightFootStepWalkSound()
        {
            PlayFootstepWalkSound(false);
        }
        
        public void PlayerLeftFootStepRunSound()
        {
            PlayFootstepRunSound(true);
        }
        
        public void PlayerRightFootStepRunSound()
        {
            PlayFootstepRunSound(false);
        }
        
        public void PlayerLeftFootStepParticle()
        {
            InvokeLeftFootStepParticleEvent();
        }
        
        public void PlayerRightFootStepParticle()
        {
            InvokeRightFootStepParticleEvent();
        }

        public void PlayerRollSound()
        {
            EventManager.AudioEvents.AudioPlayerRoll?.Invoke();
        }

        public void PlayerRollParticle()
        {
            EventManager.PlayerEvents.PlayerRollParticle?.Invoke();
        }

        public void PlayerRunningFOV()
        {
            EventManager.PlayerEvents.PlayerFOVZoomRoutine?.Invoke(105f, 0.2f, _virtualCamera);
        }

        public void PlayerRollFov()
        {
            EventManager.PlayerEvents.PlayerFOVZoomRoutine?.Invoke(105f, 0.2f, _virtualCamera);
        }
        
        public void PlayerRollEndFov()
        {
            EventManager.PlayerEvents.PlayerFOVZoomRoutine?.Invoke(75f, 0.2f, _virtualCamera);
        }

        public void PlayerCantScopeToggle()
        {
            EventManager.PlayerEvents.PlayerCanScopeToggle?.Invoke(false);
        }
        
        public void PlayerCanScopeToggle()
        {
            EventManager.PlayerEvents.PlayerCanScopeToggle?.Invoke(true);
        }

        public void PlayerCameraMoveYForFalling()
        {
            EventManager.PlayerEvents.PlayerCameraMoveY?.Invoke(0.5f, 0.75f, _virtualCamera);
        }
        
        public void PlayerCameraMoveYForGetUp()
        {
            EventManager.PlayerEvents.PlayerCameraMoveY?.Invoke(1.2f, 0.75f, _virtualCamera);
        }

        public void PlayerGetUpFromGround()
        {
            EventManager.PlayerEvents.PlayerGetUpFromGround?.Invoke();
        }

        private void SetRigWeightIncreased(bool isIncreased)
        {
            playerRigSettingsSO.RigWeightIncrease = isIncreased;
        }

        private void SetWeaponBusyState(bool isBusy)
        {
            _animator.SetBool("BusyGrabbingWeapon", isBusy);
            playerRigSettingsSO.IsGrabbingWeapon = isBusy;
        }

        private void ResetRigWeight()
        {
            _playerWeaponReloadLogic.ReturnRigWeigthToOne();
        }
        
        private void PlayerRollingEnd()
        {
           EventManager.PlayerEvents.PlayerRollingEnd?.Invoke(false);
        }
        
        private void PlayerScopeAnimation(bool isScoped)
        {
            _animator.SetBool("IsScoped", isScoped);
        }

        #region Reload Logic
        private void StartReloadProcess()
        {
            playerRigSettingsSO.IsGrabbingWeapon = true;
            _animator.SetBool("BusyGrabbingWeapon", true);
        }

        private void CompleteReloadProcess()
        {
            _playerWeaponReloadLogic.ReloadDone();
            SetRigWeightIncreased(true);
            _animator.SetBool("BusyGrabbingWeapon", false);
            playerRigSettingsSO.IsGrabbingWeapon = false;
        }
        #endregion

        #region Weapon Swap Logic
        private void StartWeaponSwap()
        {
            ResetRigWeight();
            SetWeaponBusyState(true);
        }

        private void CompleteWeaponSwap()
        {
            ResetRigWeight();
            SetWeaponBusyState(false);
        }
        #endregion

        #region Footstep Sound Logic
        private void PlayFootstepWalkSound(bool isLeftFoot)
        {
            EventManager.AudioEvents.AudioPlayerFootStepWalkSound?.Invoke(_playerWalkSound, true, 1.3f, isLeftFoot);
        }

        private void PlayFootstepRunSound(bool isLeftFoot)
        {
            EventManager.AudioEvents.AudioPlayerFootStepRunSound?.Invoke(_playerWalkSound, true, 1.3f, isLeftFoot);
        }
        #endregion

        #region Footstep Particle Logic
        private void InvokeLeftFootStepParticleEvent()
        {
            EventManager.PlayerEvents.PlayerLeftFootStepParticle?.Invoke();
        }

        private void InvokeRightFootStepParticleEvent()
        {
            EventManager.PlayerEvents.PlayerRightFootStepParticle?.Invoke();
        }
        #endregion
    }
}
