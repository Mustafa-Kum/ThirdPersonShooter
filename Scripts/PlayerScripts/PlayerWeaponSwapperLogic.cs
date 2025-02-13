using System;
using System.Collections.Generic;
using Cinemachine;
using Data;
using ScriptableObjects;
using Manager;
using UnityEngine;

public enum GrapType
{
    SideGrap,
    BackGrap
}

namespace Logic
{
    public class PlayerWeaponSwapperLogic : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponSwapperData _playerWeaponSwapperData;
        [SerializeField] private PlayerWeaponEquipAndPickUpLogic _playerWeaponEquipAndPickUpLogic;
        [SerializeField] private CinemachineVirtualCamera _thirdPersonCamera;

        private readonly Dictionary<WeaponType, int> _weaponTypeToTransformIndex = new Dictionary<WeaponType, int>
        {
            { WeaponType.Pistol, 0 },
            { WeaponType.Revolver, 1 },
            { WeaponType.Rifle, 2 },
            { WeaponType.Shotgun, 3 },
            { WeaponType.Sniper, 4 }
        };
        
        private readonly Dictionary<WeaponType, AudioSource> _weaponTypeToSwapSound = new Dictionary<WeaponType, AudioSource>();

        private void Start()
        {
            InitializeFirstWeapon();
            InitializeSwapSoundMappings();
        }   

        private void OnEnable()
        {
            SubscribeEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        /// <summary>
        /// Başlangıçta ilk silahı başlatır.
        /// </summary>
        private void InitializeFirstWeapon()
        {
            InitiateWeapon(0);
        }

        /// <summary>
        /// Eventlere abonelikleri yapar.
        /// </summary>
        private void SubscribeEvents()
        {
            EventManager.PlayerEvents.PlayerWeaponSwap += OnCharacterWeaponSwap;
        }

        /// <summary>
        /// Event aboneliklerini kaldırır.
        /// </summary>
        private void UnsubscribeEvents()
        {
            EventManager.PlayerEvents.PlayerWeaponSwap -= OnCharacterWeaponSwap;
        }

        /// <summary>
        /// Silah değiştirme event'ini ele alır.
        /// </summary>
        private void OnCharacterWeaponSwap(int weaponIndex)
        {
            InitiateWeapon(weaponIndex);
            UpdateAnimatorLayer(weaponIndex);
        }

        /// <summary>
        /// Silahı seçer, ses çalar, görselini günceller ve UI'yi günceller.
        /// </summary>
        private void InitiateWeapon(int weaponIndex)
        {
            var weaponType = GetWeaponTypeAtIndex(weaponIndex);
            DeactivateAllGuns();
            PlayWeaponSwapSound(weaponType);
            AttachLeftHand(weaponIndex);
            ActivateSelectedGun(weaponType);
            UpdateUIWeaponAlpha();
        }

        /// <summary>
        /// Verilen index'teki silah tipini döndürür.
        /// </summary>
        private WeaponType GetWeaponTypeAtIndex(int weaponIndex)
        {
            return _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO[weaponIndex].WeaponType;
        }

        /// <summary>
        /// Tüm silahları deaktif eder.
        /// </summary>
        private void DeactivateAllGuns()
        {
            foreach (var gunTransform in _playerWeaponSwapperData.GunTransforms)
            {
                gunTransform.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// UI'deki weapon alpha ayarlarını günceller.
        /// </summary>
        private void UpdateUIWeaponAlpha()
        {
            EventManager.UIEvents.UIWeaponAlpha.Invoke(_playerWeaponSwapperData.PlayerWeaponIndexSO);
        }

        /// <summary>
        /// Silah değişim seslerini sözlüğe eşler.
        /// </summary>
        private void InitializeSwapSoundMappings()
        {
            _weaponTypeToSwapSound[WeaponType.Pistol] = _playerWeaponSwapperData.WeaponSwapSound[0];
            _weaponTypeToSwapSound[WeaponType.Revolver] = _playerWeaponSwapperData.WeaponSwapSound[1];
            _weaponTypeToSwapSound[WeaponType.Rifle] = _playerWeaponSwapperData.WeaponSwapSound[2];
            _weaponTypeToSwapSound[WeaponType.Shotgun] = _playerWeaponSwapperData.WeaponSwapSound[3];
            _weaponTypeToSwapSound[WeaponType.Sniper] = _playerWeaponSwapperData.WeaponSwapSound[4];
        }
        
        /// <summary>
        /// Verilen silah tipine ait değişim sesini çalar.
        /// </summary>
        private void PlayWeaponSwapSound(WeaponType weaponType)
        {
            if (_weaponTypeToSwapSound.TryGetValue(weaponType, out AudioSource swapSoundSource))
            {
                swapSoundSource.Play();
            }
        }

        /// <summary>
        /// Seçilen silahı aktif hale getirir.
        /// </summary>
        private void ActivateSelectedGun(WeaponType weaponType)
        {
            if (_weaponTypeToTransformIndex.TryGetValue(weaponType, out int transformIndex))
            {
                _playerWeaponSwapperData.GunTransforms[transformIndex].gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Weapon type not mapped to a transform index.");
            }
        }

        /// <summary>
        /// Sol el transformunu seçilen silaha göre ayarlar.
        /// </summary>
        private void AttachLeftHand(int weaponIndex)
        {
            var weaponType = GetWeaponTypeAtIndex(weaponIndex);
            int typeIndex = (int)weaponType;

            if (IsValidHandTargetIndex(typeIndex))
            {
                SetLeftHandPositionAndRotation(typeIndex);
            }
            else
            {
                Debug.LogError("Index out of range for left hand target transforms.");
            }
        }

        /// <summary>
        /// Sol el hedef indexinin geçerli olup olmadığını kontrol eder.
        /// </summary>
        private bool IsValidHandTargetIndex(int typeIndex)
        {
            return typeIndex >= 0 && typeIndex < _playerWeaponSwapperData.LeftHandTargetTransforms.Length;
        }

        /// <summary>
        /// Sol elin pozisyon ve rotasyonunu ayarlar.
        /// </summary>
        private void SetLeftHandPositionAndRotation(int typeIndex)
        {
            _playerWeaponSwapperData.LeftHandIKTargetTransform.localPosition = 
                _playerWeaponSwapperData.LeftHandTargetTransforms[typeIndex].localPosition;
            _playerWeaponSwapperData.LeftHandIKTargetTransform.localRotation = 
                _playerWeaponSwapperData.LeftHandTargetTransforms[typeIndex].localRotation;
        }

        /// <summary>
        /// Animatör katmanlarını sıfırlar ve yeni silaha göre ayarlar.
        /// </summary>
        private void UpdateAnimatorLayer(int weaponIndex)
        {
            ResetAnimatorLayers();
            var weaponType = GetWeaponTypeAtIndex(weaponIndex);
            AdjustAnimatorSettingsBasedOnWeapon(weaponType);
        }

        /// <summary>
        /// Tüm animatör katmanlarını varsayılan hale getirir.
        /// </summary>
        private void ResetAnimatorLayers()
        {
            for (int i = 1; i < _playerWeaponSwapperData.Animator.layerCount; i++)
            {
                _playerWeaponSwapperData.Animator.SetLayerWeight(i, 0);
            }
            _playerWeaponSwapperData.Rig.weight = 0f;
        }

        /// <summary>
        /// Silah tipine göre animatör ayarlarını yapar.
        /// </summary>
        private void AdjustAnimatorSettingsBasedOnWeapon(WeaponType weaponType)
        {
            if (weaponType == WeaponType.Pistol || weaponType == WeaponType.Revolver || weaponType == WeaponType.Rifle)
            {
                SetAnimatorForWeaponType(1, GrapType.SideGrap);
            }
            else if (weaponType == WeaponType.Shotgun)
            {
                SetAnimatorForWeaponType(2, GrapType.BackGrap);
            }
            else if (weaponType == WeaponType.Sniper)
            {
                SetAnimatorForWeaponType(3, GrapType.BackGrap);
            }
        }

        /// <summary>
        /// Belirtilen katmanda grab animasyonunu tetikler ve silahın tutulduğunu işaretler.
        /// </summary>
        private void SetAnimatorForWeaponType(int layerIndex, GrapType grapType)
        {
            _playerWeaponSwapperData.Animator.SetLayerWeight(layerIndex, 1);
            PlayWeaponGrapAnimation(grapType);
            _playerWeaponSwapperData.Animator.SetTrigger("WeaponGrab");
            _playerWeaponSwapperData.Animator.SetBool("IsScoped", false);
            _playerWeaponSwapperData.PlayerRigSettingsSO.IsGrabbingWeapon = true;
    
            // Silah değiştirildikten sonra varsayılan FOV'a dön
            EventManager.PlayerEvents.PlayerFOVZoomRoutine?.Invoke(75f, 0.5f, _thirdPersonCamera);
        }

        /// <summary>
        /// Grab animasyon tipini ayarlar.
        /// </summary>
        private void PlayWeaponGrapAnimation(GrapType grapType)
        {
            _playerWeaponSwapperData.Animator.SetFloat("WeaponGrabType", (float)grapType);
        }
    }
}
