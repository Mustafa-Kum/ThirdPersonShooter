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

        private bool _isRunButtonPressed; // Run butonu basılı mı?
        private bool _wasRunning; // Önceki frame'de koşuyor muydu?

        private void Start()
        {
            InitializeComponents();
            SubscribeToEvents();
        }

        private void Update()
        {
            HandleRunningState();
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
            _isRunButtonPressed = true; // Run butonu basıldı
        }

        private void OnRunCanceled(InputAction.CallbackContext context)
        {
            _isRunButtonPressed = false; // Run butonu bırakıldı
        }

        private void SetMoveInput(Vector2 input)
        {
            _playerMovementData.PlayerMovementValueSO.MoveInput = input;
        }

        private void ResetMoveInput()
        {
            _playerMovementData.PlayerMovementValueSO.MoveInput = Vector2.zero;
            SetAnimationVelocity(0, 0);
        }

        /// <summary>
        /// Koşma durumunu günceller (Her frame'de çalışır)
        /// </summary>
        private void HandleRunningState()
        {
            bool isMovingForward = IsMovingForward();
            bool isRunningNow = _isRunButtonPressed && isMovingForward;

            // Koşma durumu değiştiyse event'leri tetikle
            if (isRunningNow != _playerMovementData.PlayerMovementValueSO.IsRunning)
            {
                _playerMovementData.PlayerMovementValueSO.IsRunning = isRunningNow;

                if (isRunningNow)
                {
                    EventManager.PlayerEvents.PlayerFOVZoomRoutine?.Invoke(105f, 0.2f, _virtualCamera);
                    EventManager.PlayerEvents.PlayerScopedAnimation?.Invoke(false);
                    EventManager.PlayerEvents.PlayerCanScopeToggle?.Invoke(false);
                    EventManager.PlayerEvents.PlayerWeaponCanFire?.Invoke(false);
                    _playerCurrentWeaponSettingsSO.IsWeaponScopped = false;
                }
                else
                {
                    EventManager.PlayerEvents.PlayerFOVZoomRoutine?.Invoke(75f, 0.2f, _virtualCamera);
                    EventManager.PlayerEvents.PlayerCanScopeToggle?.Invoke(true);
                    EventManager.PlayerEvents.PlayerWeaponCanFire?.Invoke(true);
                }
            }

            UpdateMovementSpeed();
        }

        private void UpdateMovementSpeed()
        {
            if (_playerMovementData.PlayerMovementValueSO.IsRunning)
            {
                EventManager.PlayerEvents.PlayerRunningCameraEffectStart?.Invoke();
                _playerMovementData.PlayerMovementValueSO.DefaultSpeed = _playerMovementData.PlayerMovementValueSO.RunSpeed;
            }
            else
            {
                EventManager.PlayerEvents.PlayerRunningCameraEffectEnd?.Invoke();
                _playerMovementData.PlayerMovementValueSO.DefaultSpeed = _playerMovementData.PlayerMovementValueSO.MoveSpeed;
            }
        }


        /// <summary>
        /// Karakterin ileri yönde hareket edip etmediğini kontrol eder
        /// </summary>
        private bool IsMovingForward()
        {
            Vector2 moveInput = _playerMovementData.PlayerMovementValueSO.MoveInput;
            return moveInput.y > 0.1f && Mathf.Abs(moveInput.x) < 0.5f;
        }

        private void HandleMovement()
        {
            ApplyGravity();
            MoveCharacter();
            UpdateMovementAnimations();
        }

        private void MoveCharacter()
        {
            Vector3 moveDirection = GetMoveDirection();
            if (moveDirection != Vector3.zero)
            {
                _playerMovementData.CharacterController.Move(moveDirection * 
                    (Time.deltaTime * _playerMovementData.PlayerMovementValueSO.DefaultSpeed));
            }
        }

        private Vector3 GetMoveDirection()
        {
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            
            forward.y = 0;
            right.y = 0;
            
            forward.Normalize();
            right.Normalize();
            
            Vector3 moveDirection = forward * _playerMovementData.PlayerMovementValueSO.MoveInput.y + 
                                   right * _playerMovementData.PlayerMovementValueSO.MoveInput.x;
            
            return new Vector3(moveDirection.x, _playerMovementData.PlayerMovementValueSO.VerticalVelocity, moveDirection.z);
        }

        private void ApplyGravity()
        {
            if (!_playerMovementData.CharacterController.isGrounded)
            {
                _playerMovementData.PlayerMovementValueSO.VerticalVelocity -= 
                    _playerMovementData.PlayerMovementValueSO.GravityScale * Time.deltaTime;
            }
            else
            {
                _playerMovementData.PlayerMovementValueSO.VerticalVelocity = -5f;
            }
        }

        private void UpdateMovementAnimations()
        {
            Vector3 localMoveDirection = transform.InverseTransformDirection(GetMoveDirection());
            SetAnimationVelocity(localMoveDirection.x, localMoveDirection.z);
            SetAnimationRunningState(_playerMovementData.PlayerMovementValueSO.IsRunning && 
                                     localMoveDirection.magnitude > 0);
        }

        private void SetAnimationVelocity(float xVelocity, float zVelocity)
        {
            _playerMovementData.Animator.SetFloat("xVelocity", xVelocity, 0.2f, Time.deltaTime);
            _playerMovementData.Animator.SetFloat("zVelocity", zVelocity, 0.2f, Time.deltaTime);
        }

        private void SetAnimationRunningState(bool isRunning)
        {
            _playerMovementData.Animator.SetBool("isRunning", isRunning);
        }
    }
}