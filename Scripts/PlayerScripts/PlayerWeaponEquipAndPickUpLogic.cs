using System;
using System.Collections.Generic;
using Data;
using Manager;
using ScriptableObjects;
using UnityEngine;

namespace Logic
{
    public class PlayerWeaponEquipAndPickUpLogic : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponEquipAndPickUpData _playerWeaponEquipAndPickUpData;
        
        public List<PlayerWeaponSettingsSO> PlayerWeaponSlotSO
        {
            get => _playerWeaponEquipAndPickUpData.PlayerWeaponSlotSO;
            set => _playerWeaponEquipAndPickUpData.PlayerWeaponSlotSO = value;
        }

        private void Start()
        {
            InitializeComponents();
            SubscribeToInputEvents();
            SetInitialWeapon();
            UpdateUIOnStart();
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
        /// Başlangıçta gerekli bileşenleri tanımlar.
        /// </summary>
        private void InitializeComponents()
        {
            _playerWeaponEquipAndPickUpData.PlayerControllerData = GetComponent<PlayerControllerData>();
        }

        /// <summary>
        /// Oyuncu input eventlerine abonelikleri gerçekleştirir.
        /// </summary>
        private void SubscribeToInputEvents()
        {
            var actions = _playerWeaponEquipAndPickUpData.PlayerControllerData._playerController.Character;
            actions.EquipSlot1.performed += _ => EquipWeapon(0);
            actions.EquipSlot2.performed += _ => EquipWeapon(1);
            actions.EquipSlot3.performed += _ => EquipWeapon(2);
            actions.EquipSlot4.performed += _ => EquipWeapon(3);
            actions.EquipSlot5.performed += _ => EquipWeapon(4);
            actions.DropCurrentWeapon.performed += _ => EventManager.PlayerEvents.PlayerWeaponDrop?.Invoke();
        }

        /// <summary>
        /// Karakter eventlerine abonelikleri gerçekleştirir.
        /// </summary>
        private void SubscribeToCharacterEvents()
        {
            EventManager.PlayerEvents.PlayerWeaponPickUp += PickUpWeapon;
            EventManager.PlayerEvents.PlayerWeaponDrop += DropWeapon;
        }

        /// <summary>
        /// Karakter eventlerinden abonelikleri kaldırır.
        /// </summary>
        private void UnsubscribeFromCharacterEvents()
        {
            EventManager.PlayerEvents.PlayerWeaponPickUp -= PickUpWeapon;
            EventManager.PlayerEvents.PlayerWeaponDrop -= DropWeapon;
        }

        /// <summary>
        /// Oyuncunun başlangıçta ilk silahını seçer ve UI güncellemesi yapar.
        /// </summary>
        private void SetInitialWeapon()
        {
            EquipWeapon(0);
            _playerWeaponEquipAndPickUpData.PlayerWeaponIndexSO.WeaponIndex = 0;
        }

        /// <summary>
        /// Oyuna başlarken UI güncellemelerini yapar.
        /// </summary>
        private void UpdateUIOnStart()
        {
            EventManager.UIEvents.UIWeaponAlpha.Invoke(_playerWeaponEquipAndPickUpData.PlayerWeaponIndexSO);
            EventManager.UIEvents.UIWeaponUpdate?.Invoke(
                _playerWeaponEquipAndPickUpData.PlayerWeaponSlotSO,
                _playerWeaponEquipAndPickUpData.PlayerWeaponSlotSO[_playerWeaponEquipAndPickUpData.PlayerWeaponIndexSO.WeaponIndex]
            );
        }

        /// <summary>
        /// Belirtilen indexdeki silahı kuşanır.
        /// </summary>
        private void EquipWeapon(int index)
        {
            if (!IsWeaponIndexValid(index)) return;
            
            var newWeaponSettings = PlayerWeaponSlotSO[index];
            _playerWeaponEquipAndPickUpData.CurrentWeaponSettingsSO.CopyPropertiesFrom(newWeaponSettings);
        }

        /// <summary>
        /// Silah indexinin geçerli olup olmadığını kontrol eder.
        /// </summary>
        private bool IsWeaponIndexValid(int index)
        {
            return index < PlayerWeaponSlotSO.Count;
        }

        /// <summary>
        /// Mevcut silahı bırakır.
        /// </summary>
        private void DropWeapon()
        {
            if (!CanDropWeapon()) return;

            EventManager.PlayerEvents.PlayerWeaponSwap?.Invoke(0);

            PlayerWeaponSlotSO.RemoveAt(_playerWeaponEquipAndPickUpData.PlayerWeaponIndexSO.WeaponIndex);
            
            ResetToDefaultWeapon();
            UpdateUI();
        }

        /// <summary>
        /// Silah bırakılabilir mi kontrol eder.
        /// </summary>
        private bool CanDropWeapon()
        {
            return PlayerWeaponSlotSO.Count > 1 && _playerWeaponEquipAndPickUpData.PlayerWeaponIndexSO.WeaponIndex != 0;
        }

        /// <summary>
        /// Silah bırakıldıktan sonra varsayılan silaha döner.
        /// </summary>
        private void ResetToDefaultWeapon()
        {
            EquipWeapon(0);
            _playerWeaponEquipAndPickUpData.PlayerWeaponIndexSO.WeaponIndex = 0;
        }

        /// <summary>
        /// UI'i günceller.
        /// </summary>
        private void UpdateUI()
        {
            EventManager.UIEvents.UIWeaponAlpha.Invoke(_playerWeaponEquipAndPickUpData.PlayerWeaponIndexSO);
            EventManager.UIEvents.UIWeaponUpdate?.Invoke(
                _playerWeaponEquipAndPickUpData.PlayerWeaponSlotSO,
                _playerWeaponEquipAndPickUpData.PlayerWeaponSlotSO[_playerWeaponEquipAndPickUpData.PlayerWeaponIndexSO.WeaponIndex]
            );
        }

        /// <summary>
        /// Yeni bir silah toplandığında yapılacak işlemleri yönetir.
        /// </summary>
        private void PickUpWeapon(PlayerWeaponSettingsSO newWeapon)
        {
            if (DoesPlayerHaveWeapon(newWeapon))
            {
                AddAmmoToExistingWeapon(newWeapon);
                EquipExistingWeapon(newWeapon);
                return;
            }

            TryAddNewWeaponToSlot(newWeapon);
        }

        /// <summary>
        /// Oyuncuda belirtilen silah var mı kontrol eder.
        /// </summary>
        private bool DoesPlayerHaveWeapon(PlayerWeaponSettingsSO weapon)
        {
            return PlayerWeaponSlotSO.Contains(weapon);
        }

        /// <summary>
        /// Oyuncunun elindeki mevcut bir silaha cephane ekler.
        /// </summary>
        private void AddAmmoToExistingWeapon(PlayerWeaponSettingsSO weapon)
        {
            weapon.TotalReserveAmmo += weapon.MaxAmmo;
        }

        /// <summary>
        /// Mevcut silahı kuşanır ve UI günceller.
        /// </summary>
        private void EquipExistingWeapon(PlayerWeaponSettingsSO weapon)
        {
            int weaponIndex = PlayerWeaponSlotSO.IndexOf(weapon);
            EquipWeapon(weaponIndex);
            EventManager.PlayerEvents.PlayerWeaponSwap?.Invoke(weaponIndex);
        }

        /// <summary>
        /// Yeni silahı mevcut slotlara eklemeyi dener.
        /// </summary>
        private void TryAddNewWeaponToSlot(PlayerWeaponSettingsSO newWeapon)
        {
            if (!HasFreeWeaponSlot())
            {
                Debug.LogWarning("No slots available for new weapon.");
                return;
            }

            PlayerWeaponSlotSO.Add(newWeapon);
            UpdateUI();
        }

        /// <summary>
        /// Yeni bir silah slotu var mı kontrol eder.
        /// </summary>
        private bool HasFreeWeaponSlot()
        {
            return PlayerWeaponSlotSO.Count < _playerWeaponEquipAndPickUpData.MaxSlotAllowed;
        }
    }
}
