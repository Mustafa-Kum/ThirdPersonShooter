using Data;
using PlayerScripts;
using ScriptableObjects;
using UILogic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Manager
{
    public class PlayerLogicUpdateExecuter : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponIndexSO _playerWeaponIndexSO;
        [SerializeField] private PlayerRigSettingsSO _playerRigSettingsSO;
        [SerializeField] private PlayerWeaponSettingsSO _playerCurrentWeaponSettingsSO;
        [SerializeField] private PlayerTransformValueSO _playerTransformValueSO;
        
        private PlayerControllerData _playerControllerData;
        private PlayerHealthController _playerHealthController;
        private bool _characterControlEnabled;

        private void Awake()
        {
            InitializePlayerHealthController();
        }

        private void Start()
        {
            SubscribeToCharacterEvents();
        }
        
        private void Update()
        {
            if (IsPlayerDead())
                return;

            HandleCharacterActions();
            UpdatePlayerTransformInSO();
            PerformRoll();
        }

        private void OnEnable()
        {
            SubscribeToCharacterEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromCharacterEvents();
        }

        /// <summary>
        /// PlayerHealthController bileşenini tanımlar.
        /// </summary>
        private void InitializePlayerHealthController()
        {
            _playerHealthController = GetComponent<PlayerHealthController>();
        }

        /// <summary>
        /// Oyuncu ölü mü diye kontrol eder.
        /// </summary>
        private bool IsPlayerDead()
        {
            return _playerHealthController._isDead;
        }

        /// <summary>
        /// Eventlere abonelikleri gerçekleştirir.
        /// </summary>
        private void SubscribeToCharacterEvents()
        {
            EventManager.PlayerEvents.PlayerControlBool += OnCharacterControlChanged;
            
            InitializePlayerControllerData();
            SubscribeToCharacterInputActions();
        }

        /// <summary>
        /// Event aboneliklerinden çıkar.
        /// </summary>
        private void UnsubscribeFromCharacterEvents()
        {
            EventManager.PlayerEvents.PlayerControlBool -= OnCharacterControlChanged;
            
            InitializePlayerControllerData();
            UnsubscribeFromCharacterInputActions();
        }

        /// <summary>
        /// PlayerControllerData bileşenini tanımlar.
        /// </summary>
        private void InitializePlayerControllerData()
        {
            if (_playerControllerData == null)
                _playerControllerData = GetComponent<PlayerControllerData>();
        }

        /// <summary>
        /// Oyuncu input aksiyonlarına abonelikleri tanımlar.
        /// </summary>
        private void SubscribeToCharacterInputActions()
        {
            var character = _playerControllerData._playerController.Character;
            character.Fire.performed += OnFirePerformed;
            character.Fire.canceled += OnFireCanceled;
            character.Reload.performed += OnReloadPerformed;

            character.EquipSlot1.performed += ctx => PerformWeaponSwap(0);
            character.EquipSlot2.performed += ctx => PerformWeaponSwap(1);
            character.EquipSlot3.performed += ctx => PerformWeaponSwap(2);
            character.EquipSlot4.performed += ctx => PerformWeaponSwap(3);
            character.EquipSlot5.performed += ctx => PerformWeaponSwap(4);
            
            character.WeaponBurstToggle.performed += ctx => ToggleBurstModeIfAllowed();
            character.WeaponScopeToggle.performed += ctx => EventManager.PlayerEvents.PlayerScopedAim?.Invoke();
            character.Interaction.performed += ctx => EventManager.PlayerEvents.PlayerInteractable?.Invoke();
            character.UIMissionToolTipSwitch.performed += ctx => EventManager.UIEvents.UIMissionToolTipSwitch?.Invoke();
            character.UIPause.performed += ctx => UI.instance.PauseSwitch();
        }

        /// <summary>
        /// Oyuncu input aksiyon aboneliklerini kaldırır.
        /// </summary>
        private void UnsubscribeFromCharacterInputActions()
        {
            var character = _playerControllerData._playerController.Character;
            character.Fire.performed -= OnFirePerformed;
            character.Fire.canceled -= OnFireCanceled;
            character.Reload.performed -= OnReloadPerformed;
        }

        private void OnCharacterControlChanged(bool isEnabled)
        {
            _characterControlEnabled = isEnabled;
        }

        /// <summary>
        /// Karakterin etkin kontrolü varsa yapılacak aksiyonları kontrol eder.
        /// </summary>
        private void HandleCharacterActions()
        {
            if (!_characterControlEnabled)
                return;

            InvokeCharacterMovementEvents();
            CheckAndInvokeFireEvent();
            CheckAndInvokeSlowMotion();
        }

        /// <summary>
        /// Karakter hareketi ve aim eventlerini tetikler.
        /// </summary>
        private void InvokeCharacterMovementEvents()
        {
            EventManager.PlayerEvents.PlayerMovement?.Invoke();
            EventManager.PlayerEvents.PlayerAim?.Invoke();
        }

        /// <summary>
        /// Oyuncu ateş ediyorsa CharacterFire eventini tetikler.
        /// </summary>
        private void CheckAndInvokeFireEvent()
        {
            if (_playerCurrentWeaponSettingsSO.IsShooting)
                EventManager.PlayerEvents.PlayerFire?.Invoke();
        }

        /// <summary>
        /// Q tuşuna basılırsa SlowMotion eventini tetikler.
        /// </summary>
        private void CheckAndInvokeSlowMotion()
        {
            if (Input.GetKeyDown(KeyCode.Q))
                EventManager.PlayerEvents.PlayerSlowMotion?.Invoke(1f);
        }

        /// <summary>
        /// Oyuncu pozisyonunu PlayerTransformValueSO'ya aktarır.
        /// </summary>
        private void UpdatePlayerTransformInSO()
        {
            _playerTransformValueSO.PlayerTransform = transform.position;
        }

        private void PerformRoll()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                EventManager.PlayerEvents.PlayerRollingStart?.Invoke(true);
        }

        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            SetWeaponShootingState(true);
            CheckIfOutOfAmmo();
        }

        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            SetWeaponShootingState(false);
        }

        /// <summary>
        /// Silah atış durumunu günceller.
        /// </summary>
        private void SetWeaponShootingState(bool isShooting)
        {
            _playerCurrentWeaponSettingsSO.IsShooting = isShooting;
        }

        /// <summary>
        /// Mermi yoksa ilgili sesleri çalar.
        /// </summary>
        private void CheckIfOutOfAmmo()
        {
            if (_playerCurrentWeaponSettingsSO.WeaponAmmo <= 0)
            {
                EventManager.AudioEvents.AudioWeaponOutOfAmmoSound?.Invoke(_playerCurrentWeaponSettingsSO.WeaponType);
                EventManager.AudioEvents.VoiceOutOfAmmo?.Invoke();
            }
        }

        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            TryReloadWeapon();
        }

        /// <summary>
        /// Reload yapabiliyorsa Reload eventini tetikler.
        /// </summary>
        private void TryReloadWeapon()
        {
            if (!_characterControlEnabled)
                return;

            if (_playerCurrentWeaponSettingsSO.CanReload())
                EventManager.PlayerEvents.PlayerWeaponReload?.Invoke(_playerCurrentWeaponSettingsSO.WeaponType);
        }

        private void PerformWeaponSwap(int weaponIndex)
        {
            if (!_characterControlEnabled)
                return;
            
            _playerWeaponIndexSO.WeaponIndex = weaponIndex;
            EventManager.PlayerEvents.PlayerWeaponSwap?.Invoke(weaponIndex);
        }
        
        /// <summary>
        /// Shotgun değilse burst modu değiştirir.
        /// </summary>
        private void ToggleBurstModeIfAllowed()
        {
            if (_playerCurrentWeaponSettingsSO.WeaponType != WeaponType.Shotgun)
                _playerCurrentWeaponSettingsSO.ToggleBurstMode();
        }
    }
}
