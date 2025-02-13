using System.Collections.Generic;
using Manager;
using ScriptableObjects;
using UnityEngine;

namespace Logic
{
    public class AmmoBoxPickedUpLogic : MonoBehaviour
    {
        [SerializeField] private List<AmmoData> _smallAmmoBox;
        [SerializeField] private PlayerWeaponSettingsSO[] _playerWeaponSettingsSOs;
        [SerializeField] private PlayerWeaponSettingsSO _playerCurrentWeaponSettingsSO;

        // Silah tipine karşılık gelen silah ayarlarını hızlı erişmek için kullanacağımız bir sözlük.
        private Dictionary<WeaponType, PlayerWeaponSettingsSO> _weaponAmmoMapping;

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
            InitializeWeaponAmmoMapping();
        }

        private void SubscribeEvents()
        {
            EventManager.PlayerEvents.PlayerAmmoPickedUp += AmmoPickedUp;
        }

        private void UnsubscribeEvents()
        {
            EventManager.PlayerEvents.PlayerAmmoPickedUp -= AmmoPickedUp;
        }

        private void InitializeWeaponAmmoMapping()
        {
            _weaponAmmoMapping = new Dictionary<WeaponType, PlayerWeaponSettingsSO>();

            // Elimizdeki PlayerWeaponSettingsSO'ların WeaponType'larını bir sözlüğe alarak
            // ammo güncellemesini daha açık hale getiriyoruz.
            foreach (var weaponSettings in _playerWeaponSettingsSOs)
            {
                if (!_weaponAmmoMapping.ContainsKey(weaponSettings.WeaponType))
                {
                    _weaponAmmoMapping.Add(weaponSettings.WeaponType, weaponSettings);
                }
            }
        }

        private void AmmoPickedUp()
        {
            // Her tip ammo için silahları güncelle
            bool ammoAddedToCurrentWeapon = false;
            foreach (var smallAmmo in _smallAmmoBox)
            {
                UpdateWeaponAmmoIfExists(smallAmmo, ref ammoAddedToCurrentWeapon);
            }
        }

        private void UpdateWeaponAmmoIfExists(AmmoData smallAmmo, ref bool ammoAddedToCurrentWeapon)
        {
            // Eğer bu weapon type için bir mapping varsa ammo ekle
            if (_weaponAmmoMapping.TryGetValue(smallAmmo._weaponType, out var weaponSettings))
            {
                UpdateTotalReservedAmmo(weaponSettings, smallAmmo._ammoAmount);

                // Sadece bir kere current weapon'a ammo ekliyoruz
                if (!ammoAddedToCurrentWeapon)
                {
                    _playerCurrentWeaponSettingsSO.TotalReserveAmmo += smallAmmo._ammoAmount;
                    ammoAddedToCurrentWeapon = true;
                }
            }
        }

        private void UpdateTotalReservedAmmo(PlayerWeaponSettingsSO weaponSettings, int ammoAmount)
        {
            weaponSettings.TotalReserveAmmo += ammoAmount;
        }
    }

    [System.Serializable]
    public struct AmmoData
    {
        public WeaponType _weaponType;
        public int _ammoAmount;
    }
}