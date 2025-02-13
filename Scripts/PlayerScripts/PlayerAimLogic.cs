using System.Collections;
using Cinemachine;
using Data;
using Manager;
using UnityEngine;

namespace Logic
{
    public class PlayerAimLogic : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerAimData _playerAimData;
        [SerializeField] private CinemachineVirtualCamera _thirdPersonCamera;
        [SerializeField] private LayerMask _layerMask;

        [Header("Settings")]
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private float _cameraTargetSmoothSpeed;
        [SerializeField] private float _toggleCooldown = 0.5f;

        [Header("Mouse Settings")]
        [SerializeField] private float _mouseSensitivity = 100f;
        
        [Header("Recoil Settings")]
        [SerializeField] private float _recoilRecoverySpeed = 5f; // Recoil'den geri dönüş hızı
        
        private float _xRotation = 0f;
        private float _currentRecoilX = 0f;
        private float _currentRecoilY = 0f;
        private float _lastToggleTime = -Mathf.Infinity;
        private bool _cantToggle = true;

        private void Awake()
        {
            InitializeDependencies();
        }

        private void InitializeDependencies()
        {
            _playerAimData.PlayerControllerData = GetComponent<PlayerControllerData>();
        }

        private void Start()
        {
            AssignInputEvents();
            HideCursor();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            HandleMouseInput();
            HandleRecoilRecovery();
        }

        #region Event Subscription

        private void SubscribeToEvents()
        {
            EventManager.PlayerEvents.PlayerAim += HandleCharacterAim;
            EventManager.PlayerEvents.PlayerScopedAim += ToggleScopedAim;
            EventManager.PlayerEvents.PlayerWeaponRecoil += ApplyRecoil;
            EventManager.PlayerEvents.PlayerCanScopeToggle += CantToggleEvent;
        }

        private void UnsubscribeFromEvents()
        {
            EventManager.PlayerEvents.PlayerAim -= HandleCharacterAim;
            EventManager.PlayerEvents.PlayerScopedAim -= ToggleScopedAim;
            EventManager.PlayerEvents.PlayerWeaponRecoil -= ApplyRecoil;
            EventManager.PlayerEvents.PlayerCanScopeToggle -= CantToggleEvent;
        }

        #endregion

        #region Input Handling

        private void AssignInputEvents()
        {
            var aimInputAction = _playerAimData.PlayerControllerData._playerController.Character.Aim;
            aimInputAction.performed += context => SetAimInput(context.ReadValue<Vector2>());
            aimInputAction.canceled += context => ResetAimInput();
        }

        private void SetAimInput(Vector2 input)
        {
            _playerAimData.PlayerAimSo.AimInput = input;
        }

        private void ResetAimInput()
        {
            _playerAimData.PlayerAimSo.AimInput = Vector2.zero;
        }

        #endregion

        #region Aiming Logic

        private void HandleCharacterAim()
        {
            UpdateAiming();
        }

        private void UpdateAiming()
        {
            UpdateCameraTargetPosition();
        }

        private void UpdateCameraTargetPosition()
        {
            RaycastHit hitInfo;
            Ray ray = new Ray(_thirdPersonCamera.transform.position, _thirdPersonCamera.transform.forward);
            Vector3 targetPosition;

            if (Physics.Raycast(ray, out hitInfo, 50f, _layerMask))
            {
                targetPosition = hitInfo.point;
            }
            else
            {
                targetPosition = ray.origin + ray.direction * 50f;
            }
            
            _playerAimData.CameraTarget.position = Vector3.Lerp(
                _playerAimData.CameraTarget.position, 
                targetPosition, 
                _cameraTargetSmoothSpeed * Time.deltaTime
            );

            UpdateCameraRotation(_playerAimData.CameraTarget.position);
        }
        
        private void UpdateCameraRotation(Vector3 targetPoint)
        {
            Vector3 direction = targetPoint - _thirdPersonCamera.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            _thirdPersonCamera.transform.rotation = Quaternion.Lerp(
                _thirdPersonCamera.transform.rotation,
                targetRotation,
                _smoothSpeed * Time.deltaTime
            );
        }

        #endregion

        #region Scope Camera Handling

        private void ToggleScopedAim()
        {
            if (_cantToggle == false)
                return;

            if (IsToggleCooldownActive())
                return;

            _lastToggleTime = Time.time;
            _playerAimData.PlayerCurrentWeaponSettingsSO.IsWeaponScopped = !_playerAimData.PlayerCurrentWeaponSettingsSO.IsWeaponScopped;

            if (_playerAimData.PlayerCurrentWeaponSettingsSO.IsWeaponScopped)
            {
                EventManager.PlayerEvents.PlayerFOVZoomRoutine?.Invoke(40f, 0.2f, _thirdPersonCamera);
            }
            else
            {
                EventManager.PlayerEvents.PlayerFOVZoomRoutine?.Invoke(75f, 0.2f, _thirdPersonCamera);
            }
            
            InvokeAudioAndAnimationEvents();

            StartCoroutine(EnableWeaponFireAfterScopeToggle());
        }

        private bool IsToggleCooldownActive()
        {
            return Time.time - _lastToggleTime < _toggleCooldown;
        }

        private void CantToggleEvent(bool cantToggle)
        {
            _cantToggle = cantToggle;
        }

        private void InvokeAudioAndAnimationEvents()
        {
            if (_playerAimData.PlayerCurrentWeaponSettingsSO.IsWeaponScopped)
            {
                EventManager.AudioEvents.AudioWeaponScopeInSound?.Invoke();
                EventManager.PlayerEvents.PlayerScopedAnimation?.Invoke(true);
            }
            else
            {
                EventManager.AudioEvents.AudioWeaponScopeOutSound?.Invoke();
                EventManager.PlayerEvents.PlayerScopedAnimation?.Invoke(false);
            }
        }

        private IEnumerator EnableWeaponFireAfterScopeToggle()
        {
            EventManager.PlayerEvents.PlayerWeaponCanFire?.Invoke(false);
            yield return new WaitForSeconds(0.6f);
            EventManager.PlayerEvents.PlayerWeaponCanFire?.Invoke(true);
        }

        #endregion

        #region Recoil Logic

        // Bu metot ateş edildikten sonra çağrılır. (PlayerFiredWeapon event'ine bağlı.)
        private void ApplyRecoil(float recoilAmountX, float recoilAmountY)
        {
            // Kameraya yukarı doğru bir sarsıntı ver
            _currentRecoilX += -recoilAmountX;
            _currentRecoilY += Random.Range(-recoilAmountY, recoilAmountY);
        }

        private void HandleRecoilRecovery()
        {
            // currentRecoilX ve currentRecoilY değerlerini zamanla 0'a döndür
            _currentRecoilX = Mathf.Lerp(_currentRecoilX, 0f, Time.deltaTime * _recoilRecoverySpeed);
            _currentRecoilY = Mathf.Lerp(_currentRecoilY, 0f, Time.deltaTime * _recoilRecoverySpeed);
        }

        #endregion

        #region Mouse Input Handling

        private void HandleMouseInput()
        {
            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

            // Yukarı ve aşağı hareket (pitch) + recoil etkisi
            _xRotation -= mouseY;
            _xRotation += _currentRecoilX; // Recoil'den gelen ekstra rotasyon
            _xRotation = Mathf.Clamp(_xRotation, -30f, 30f);

            // Sağ ve sola hareket (yaw) + recoil etkisi
            float finalYRotation = mouseX + _currentRecoilY; 

            // Kamerayı uygula
            _thirdPersonCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * finalYRotation);
        }
        
        private void HideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        #endregion
    }
}
