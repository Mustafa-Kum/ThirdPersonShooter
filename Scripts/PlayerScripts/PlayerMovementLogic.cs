using System;
using Cinemachine;
using Data;
using UnityEngine;
using Manager;
using ScriptableObjects;
using UnityEngine.InputSystem;

namespace Logic
{
    public class PlayerMovementLogic : MonoBehaviour
    {
        [SerializeField] private PlayerMovementData _playerMovementData;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private PlayerWeaponSettingsSO _playerCurrentWeaponSettingsSO;

        private void Start()
        {
            InitializeComponents();
            SubscribeToEvents();
        }

        private void Update()
        {
            StopRunningCheck();

            if (_playerMovementData.PlayerMovementValueSO.IsRunning == true)
                EventManager.PlayerEvents.PlayerRunningCameraEffectStart?.Invoke();
        }

        private void InitializeComponents()
        {
            _playerMovementData.PlayerControllerData = GetComponent<PlayerControllerData>();
            _playerMovementData.CharacterController ??= GetComponent<CharacterController>();
            _playerMovementData.Animator ??= GetComponentInChildren<Animator>();
        }

        private void SubscribeToEvents()
        {
            var playerControls = _playerMovementData.PlayerControllerData._playerController.Character;
            playerControls.Movement.performed += OnMovementPerformed;
            playerControls.Movement.canceled += OnMovementCanceled;
            playerControls.Run.performed += OnRunPerformed;
            playerControls.Run.canceled += OnRunCanceled;
        }

        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerMovement += HandleMovement;
        }

        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerMovement -= HandleMovement;
        }

        private void OnMovementPerformed(InputAction.CallbackContext context)
        {
            SetMoveInput(context.ReadValue<Vector2>());
        }

        private void OnMovementCanceled(InputAction.CallbackContext context)
        {
            ResetMoveInput();
        }

        private void OnRunPerformed(InputAction.CallbackContext context)
        {
            StartRunning();
        }

        private void OnRunCanceled(InputAction.CallbackContext context)
        {
            StopRunning();
        }

        /// <summary>
        /// Hareket girdi değerlerini ayarlar.
        /// </summary>
        private void SetMoveInput(Vector2 input)
        {
            _playerMovementData.PlayerMovementValueSO.MoveInput = input;
        }

        /// <summary>
        /// Hareket girdi değerlerini sıfırlar ve animasyon hızlarını durdurur.
        /// </summary>
        private void ResetMoveInput()
        {
            _playerMovementData.PlayerMovementValueSO.MoveInput = Vector2.zero;
            SetAnimationVelocity(0, 0);
        }

        /// <summary>
        /// Koşmayı başlatır ve hızı günceller.
        /// </summary>
        private void StartRunning()
        {
            _playerMovementData.PlayerMovementValueSO.IsRunning = true;
            EventManager.PlayerEvents.PlayerScopedAnimation?.Invoke(false);
            EventManager.PlayerEvents.PlayerCanScopeToggle?.Invoke(false);
            EventManager.PlayerEvents.PlayerWeaponCanFire?.Invoke(false);
            _playerCurrentWeaponSettingsSO.IsWeaponScopped = false;
            UpdateMovementSpeed();
        }

        /// <summary>
        /// Koşmayı durdurur ve hızı günceller.
        /// </summary>
        private void StopRunning()
        {
            if (_playerMovementData.PlayerMovementValueSO.IsRolling)
                return;
            
            _playerMovementData.PlayerMovementValueSO.IsRunning = false;
            EventManager.PlayerEvents.PlayerFOVZoomRoutine?.Invoke(75f, 0.2f, _virtualCamera);
            EventManager.PlayerEvents.PlayerCanScopeToggle?.Invoke(true);
            EventManager.PlayerEvents.PlayerWeaponCanFire?.Invoke(true);
            EventManager.PlayerEvents.PlayerRunningCameraEffectEnd?.Invoke();
            UpdateMovementSpeed();
        }

        private void StopRunningCheck()
        {
            if (_playerMovementData.PlayerMovementValueSO.MoveInput.y == 0 && _playerMovementData.PlayerMovementValueSO.MoveInput.x == 0)
            {
                _playerMovementData.PlayerMovementValueSO.IsRunning = false;
            }
        }

        /// <summary>
        /// Koşma/normal hız değerlerini günceller.
        /// </summary>
        private void UpdateMovementSpeed()
        {
            _playerMovementData.PlayerMovementValueSO.DefaultSpeed = _playerMovementData.PlayerMovementValueSO.IsRunning ? 
                _playerMovementData.PlayerMovementValueSO.RunSpeed : _playerMovementData.PlayerMovementValueSO.MoveSpeed;
        }

        /// <summary>
        /// Hareket işlemlerini (Yer çekimi, hareket, animasyon) yönetir.
        /// </summary>
        private void HandleMovement()
        {
            ApplyGravity();
            MoveCharacter();
            UpdateMovementAnimations();
        }

        /// <summary>
        /// Karakteri girdilere göre hareket ettirir.
        /// </summary>
        private void MoveCharacter()
        {
            Vector3 moveDirection = GetMoveDirection();
            if (moveDirection != Vector3.zero)
            {
                _playerMovementData.CharacterController.Move(moveDirection * (Time.deltaTime * _playerMovementData.PlayerMovementValueSO.DefaultSpeed));
            }
        }

        /// <summary>
        /// Karakterin ileri-geri-sağa-sola hareket vektörünü hesaplar.
        /// </summary>
        private Vector3 GetMoveDirection()
        {
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            
            forward.y = 0;
            right.y = 0;
            
            forward.Normalize();
            right.Normalize();
            
            Vector3 moveDirection = forward * _playerMovementData.PlayerMovementValueSO.MoveInput.y + right * _playerMovementData.PlayerMovementValueSO.MoveInput.x;
            
            return new Vector3(moveDirection.x, _playerMovementData.PlayerMovementValueSO.VerticalVelocity, moveDirection.z);
        }

        /// <summary>
        /// Yer çekimi uygular.
        /// </summary>
        private void ApplyGravity()
        {
            if (!_playerMovementData.CharacterController.isGrounded)
            {
                _playerMovementData.PlayerMovementValueSO.VerticalVelocity -= _playerMovementData.PlayerMovementValueSO.GravityScale * Time.deltaTime;
            }
            else
            {
                _playerMovementData.PlayerMovementValueSO.VerticalVelocity = -5f;
            }
        }

        /// <summary>
        /// Hareketle ilgili animasyon parametrelerini günceller.
        /// </summary>
        private void UpdateMovementAnimations()
        {
            Vector3 localMoveDirection = transform.InverseTransformDirection(GetMoveDirection());
            SetAnimationVelocity(localMoveDirection.x, localMoveDirection.z);
            SetAnimationRunningState(_playerMovementData.PlayerMovementValueSO.IsRunning && localMoveDirection.magnitude > 0);
        }

        /// <summary>
        /// Animasyon hız parametrelerini ayarlar.
        /// </summary>
        private void SetAnimationVelocity(float xVelocity, float zVelocity)
        {
            _playerMovementData.Animator.SetFloat("xVelocity", xVelocity, 0.2f, Time.deltaTime);
            _playerMovementData.Animator.SetFloat("zVelocity", zVelocity, 0.2f, Time.deltaTime);
        }

        /// <summary>
        /// Koşma durumunu animasyona aktarır.
        /// </summary>
        private void SetAnimationRunningState(bool isRunning)
        {
            _playerMovementData.Animator.SetBool("isRunning", isRunning);
        }
    }
}
