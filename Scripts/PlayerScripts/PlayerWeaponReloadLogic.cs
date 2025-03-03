using System;
using System.Collections.Generic;
using Data;
using Manager;
using ScriptableObjects;
using UnityEngine;

namespace Logic
{
    public class PlayerWeaponReloadLogic : MonoBehaviour
    {
        public PlayerWeaponReloadData _playerWeaponReloadData;
        
        [SerializeField] private PlayerWeaponEquipAndPickUpLogic _playerWeaponEquipAndPickUpLogic;
        
        private readonly Dictionary<WeaponType, AudioSource> _weaponTypeToReloadSound = new Dictionary<WeaponType, AudioSource>();

        private void Start()
        {
            InitializeReloadSoundMappings();
        }

        private void Update()
        {
            UpdateRigWeight();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Eventlere abonelikleri yapar.
        /// </summary>
        private void SubscribeToEvents()
        {
            EventManager.PlayerEvents.PlayerWeaponReload += OnCharacterWeaponReload;
        }

        /// <summary>
        /// Event aboneliklerini kaldırır.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            EventManager.PlayerEvents.PlayerWeaponReload -= OnCharacterWeaponReload;
        }

        /// <summary>
        /// Karakter silah yeniden doldurma event tetiklendiğinde çağrılır.
        /// </summary>
        private void OnCharacterWeaponReload(WeaponType weaponType)
        {
            HandleWeaponReload(weaponType);
        }

        /// <summary>
        /// Silah yeniden doldurma işlemini gerçekleştirir.
        /// </summary>
        private void HandleWeaponReload(WeaponType weaponType)
        {
            if (IsWeaponCurrentlyGrabbing())
                return;

            PlayWeaponReloadSound(weaponType);
            SetAnimatorForReload();
            PrepareRigForReload();
        }

        /// <summary>
        /// Silah tutma animasyonu devrede mi kontrol eder.
        /// </summary>
        private bool IsWeaponCurrentlyGrabbing()
        {
            return _playerWeaponReloadData.PlayerRigSettingsSO.IsGrabbingWeapon;
        }

        /// <summary>
        /// Animator reload animasyonu için gerekli parametreleri ayarlar.
        /// </summary>
        private void SetAnimatorForReload()
        {
            _playerWeaponReloadData.Animator.SetFloat("ReloadSpeed", _playerWeaponReloadData.CurrentWeaponSettingsSO.ReloadSpeed);
            _playerWeaponReloadData.Animator.SetTrigger("Reload");
        }

        /// <summary>
        /// Reload öncesi Rig ağırlığını ayarlar.
        /// </summary>
        private void PrepareRigForReload()
        {
            _playerWeaponReloadData.Rig.weight = 0.15f;
        }

        /// <summary>
        /// Rig ağırlığını kademeli olarak günceller.
        /// </summary>
        private void UpdateRigWeight()
        {
            if (_playerWeaponReloadData.PlayerRigSettingsSO.RigWeightIncrease)
            {
                IncreaseRigWeightOverTime();
            }
        }

        /// <summary>
        /// Rig ağırlığını zamanla 1'e arttırır.
        /// </summary>
        private void IncreaseRigWeightOverTime()
        {
            _playerWeaponReloadData.Rig.weight += _playerWeaponReloadData.RigIncreaseSpeed * Time.deltaTime;
            
            if (_playerWeaponReloadData.Rig.weight >= 1)
            {
                _playerWeaponReloadData.PlayerRigSettingsSO.RigWeightIncrease = false;
            }
        }

        public void ReturnRigWeigthToOne()
        {
            _playerWeaponReloadData.PlayerRigSettingsSO.RigWeightIncrease = true;
        }

        public void ReloadDone()
        {
            PerformReloadCompletionActions();
        }

        /// <summary>
        /// Reload tamamlandığında cephane yenileme, ses durdurma ve UI güncellemelerini yapar.
        /// </summary>
        private void PerformReloadCompletionActions()
        {
            int activeWeaponIndex = GetValidWeaponIndex();
            ReloadAmmunitionForActiveWeapon(activeWeaponIndex);
            StopReloadSound();
            UpdateUIAfterReload(activeWeaponIndex);
        }


        /// <summary>
        /// Aktif silahın cephanesini yeniler.
        /// </summary>
        private void ReloadAmmunitionForActiveWeapon(int activeWeaponIndex)
        {
            if (activeWeaponIndex < 0 || activeWeaponIndex >= _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO.Count)
            {
                Debug.LogError($"Active weapon index {activeWeaponIndex} is out of range. Total available weapons: {_playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO.Count}");
                return;
            }
            
            _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO[activeWeaponIndex].ReloadAmmo();
            _playerWeaponReloadData.CurrentWeaponSettingsSO.ReloadAmmo();
        }

        private int GetValidWeaponIndex()
        {
            int index = _playerWeaponReloadData.PlayerWeaponIndexSO.WeaponIndex;
            if (index < 0 || index >= _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO.Count)
            {
                // Default to pistol: set the current weapon type and index to 0.
                _playerWeaponReloadData.CurrentWeaponSettingsSO.WeaponType = WeaponType.Pistol;
                _playerWeaponReloadData.PlayerWeaponIndexSO.WeaponIndex = 0;
                index = 0;
            }
            return index;
        }



        /// <summary>
        /// Reload sesi çalıyorsa durdurur.
        /// </summary>
        private void StopReloadSound()
        {
            _weaponTypeToReloadSound[_playerWeaponReloadData.CurrentWeaponSettingsSO.WeaponType].Stop();
        }

        /// <summary>
        /// Reload sonrası UI'yi günceller.
        /// </summary>
        private void UpdateUIAfterReload(int activeWeaponIndex)
        {
            activeWeaponIndex = GetValidWeaponIndex();
            EventManager.UIEvents.UIWeaponUpdate?.Invoke(
                _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO,
                _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO[activeWeaponIndex]
            );
        }


        /// <summary>
        /// Her bir weapon type için reload ses kaynağını sözlükte eşler.
        /// </summary>
        private void InitializeReloadSoundMappings()
        {
            _weaponTypeToReloadSound[WeaponType.Pistol] = _playerWeaponReloadData.WeaponReloadSound[0];
            _weaponTypeToReloadSound[WeaponType.Revolver] = _playerWeaponReloadData.WeaponReloadSound[1];
            _weaponTypeToReloadSound[WeaponType.Rifle] = _playerWeaponReloadData.WeaponReloadSound[2];
            _weaponTypeToReloadSound[WeaponType.Shotgun] = _playerWeaponReloadData.WeaponReloadSound[3];
            _weaponTypeToReloadSound[WeaponType.Sniper] = _playerWeaponReloadData.WeaponReloadSound[4];
        }

        /// <summary>
        /// Belirtilen silah türü için reload sesini çalar.
        /// </summary>
        private void PlayWeaponReloadSound(WeaponType weaponType)
        {
            if (_weaponTypeToReloadSound.TryGetValue(weaponType, out AudioSource reloadSoundSource))
            {
                reloadSoundSource.Play();
            }
        }
    }
}
