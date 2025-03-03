using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Lean.Pool;
using Logic;
using Manager;
using ScriptableObjects;
using UnityEngine;

namespace Controller
{
    public class PlayerWeaponFireLogic : MonoBehaviour
    {
        private const float ReferenceBulletSpeed = 20f;

        [SerializeField] private PlayerWeaponFireData _playerWeaponFireData;
        [SerializeField] private PlayerWeaponEquipAndPickUpLogic _playerWeaponEquipAndPickUpLogic;
        [SerializeField] private bool _canFire = true;

        private readonly Dictionary<WeaponType, int> _weaponTypeToIndex = new Dictionary<WeaponType, int>
        {
            { WeaponType.Pistol, 0 },
            { WeaponType.Revolver, 1 },
            { WeaponType.Rifle, 2 },
            { WeaponType.Shotgun, 3 },
            { WeaponType.Sniper, 4 }
        };

        private readonly Dictionary<WeaponType, ParticleSystem> _weaponTypeToMuzzleFlash = new Dictionary<WeaponType, ParticleSystem>();
        private readonly Dictionary<WeaponType, Transform> _weaponTypeToGunPoint = new Dictionary<WeaponType, Transform>();

        private float _scopedFireTimer = 0f;
        private bool _isScopedFireDelayed = false;

        private void Start()
        {
            InitializeAnimator();
            InitializeMappings();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Update()
        {
            HandleFireAnimation();
            BulletDirection();
        }

        #region Event Subscription

        private void SubscribeEvents()
        {
            EventManager.PlayerEvents.PlayerFire += Fire;
            EventManager.PlayerEvents.PlayerWeaponSwap += InitializeMappings;
            EventManager.PlayerEvents.PlayerWeaponCanFire += SetCanFire;
        }

        private void UnsubscribeEvents()
        {
            EventManager.PlayerEvents.PlayerFire -= Fire;
            EventManager.PlayerEvents.PlayerWeaponSwap -= InitializeMappings;
            EventManager.PlayerEvents.PlayerWeaponCanFire -= SetCanFire;
        }

        #endregion

        #region Initialization

        private void InitializeAnimator()
        {
            _playerWeaponFireData.Animator = GetComponentInChildren<Animator>();
        }

        private void InitializeMappings(int weaponIndex = -1)
        {
            _weaponTypeToMuzzleFlash[WeaponType.Pistol] = _playerWeaponFireData.MuzzleFlash[0];
            _weaponTypeToMuzzleFlash[WeaponType.Revolver] = _playerWeaponFireData.MuzzleFlash[1];
            _weaponTypeToMuzzleFlash[WeaponType.Rifle] = _playerWeaponFireData.MuzzleFlash[2];
            _weaponTypeToMuzzleFlash[WeaponType.Shotgun] = _playerWeaponFireData.MuzzleFlash[3];
            _weaponTypeToMuzzleFlash[WeaponType.Sniper] = _playerWeaponFireData.MuzzleFlash[4];

            _weaponTypeToGunPoint[WeaponType.Pistol] = _playerWeaponFireData.GunPoint[0];
            _weaponTypeToGunPoint[WeaponType.Revolver] = _playerWeaponFireData.GunPoint[1];
            _weaponTypeToGunPoint[WeaponType.Rifle] = _playerWeaponFireData.GunPoint[2];
            _weaponTypeToGunPoint[WeaponType.Shotgun] = _playerWeaponFireData.GunPoint[3];
            _weaponTypeToGunPoint[WeaponType.Sniper] = _playerWeaponFireData.GunPoint[4];
        }

        #endregion

        #region Fire Logic

        private void Fire()
        {
            if (ShouldPreventFire())
                return;

            var currentWeaponSettings = _playerWeaponFireData.CurrentWeaponSettingsSO;

            if (IsSingleShootType(currentWeaponSettings))
            {
                currentWeaponSettings.IsShooting = false;
            }

            if (currentWeaponSettings.BurstActivated())
            {
                StartCoroutine(BurstFire());
                return;
            }

            FireSingleBullet();
        }

        private bool ShouldPreventFire()
        {
            var playerRig = _playerWeaponFireData.PlayerRigSettingsSO;
            var currentWeaponSettings = _playerWeaponFireData.CurrentWeaponSettingsSO;

            return playerRig.IsGrabbingWeapon || !currentWeaponSettings.CanShoot() || !_canFire;
        }

        private bool IsSingleShootType(PlayerWeaponSettingsSO weaponSettings)
        {
            int index = _playerWeaponFireData.PlayerWeaponIndexSO.WeaponIndex;

            // Check if index is within the bounds of the list
            if (index < 0 || index >= _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO.Count)
            {
                _playerWeaponFireData.CurrentWeaponSettingsSO.WeaponType = WeaponType.Pistol;
                return false;
            }

            // Get the weapon slot from the list safely
            var weaponSlot = _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO[index];

            // If the shoot type is not single, default to Pistol
            if (weaponSlot.ShootType != ShootType.Single)
            {
                _playerWeaponFireData.CurrentWeaponSettingsSO.WeaponType = WeaponType.Rifle;
                return false;
            }
            
            return true;
        }


        private void FireSingleBullet()
        {
            var currentWeaponSettings = _playerWeaponFireData.CurrentWeaponSettingsSO;
            ProcessFire(currentWeaponSettings.WeaponType);

            currentWeaponSettings.IncreaseSpread();
            PlayFireSound();
            ApplyRecoil();
            ApplyCameraShake();
            ApplyZoomRoutine();
            UpdateUIAmmoCount();
        }

        private void ProcessFire(WeaponType weaponType)
        {
            var currentWeaponSettings = _playerWeaponFireData.CurrentWeaponSettingsSO;

            if (currentWeaponSettings.WeaponAmmo <= 0)
                return;

            currentWeaponSettings.WeaponAmmo--;

            var bullet = SpawnBullet(weaponType);
            ConfigureBullet(bullet, weaponType);

            // Sadece Fire animasyonu tetikleniyor
            _playerWeaponFireData.Animator.SetTrigger("Fire");
            _playerWeaponFireData.Animator.SetTrigger("ScopedFireTrigger");

            TriggerMuzzleFlash(weaponType);
            UpdateAmmoCount();
        }

        private GameObject SpawnBullet(WeaponType weaponType)
        {
            int index = _weaponTypeToIndex[weaponType];
            GameObject bulletPrefab = _playerWeaponFireData.PlayerBulletSettingsSO.BulletPrefabs[index];
            return LeanPool.Spawn(
                bulletPrefab,
                _playerWeaponFireData.GunPoint[index].position,
                Quaternion.LookRotation(_playerWeaponFireData.GunPoint[index].forward)
            );
        }

        private void ConfigureBullet(GameObject bullet, WeaponType weaponType)
        {
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            PlayerBulletLogic bulletLogic = bullet.GetComponent<PlayerBulletLogic>();

            bulletLogic.BulletSetup(_playerWeaponFireData.CurrentWeaponSettingsSO.GunRange, _playerWeaponFireData.PlayerBulletSettingsSO.BulletImpactForce);

            rb.mass = ReferenceBulletSpeed * _playerWeaponFireData.PlayerBulletSettingsSO.BulletSpeed;
            rb.velocity = CalculateBulletSpeed(weaponType);
            LeanPool.Despawn(bullet, 10f); // Bullets despawn after 10 seconds
        }

        private Vector3 CalculateBulletSpeed(WeaponType weaponType)
        {
            var currentWeaponSettings = _playerWeaponFireData.CurrentWeaponSettingsSO;
            Vector3 direction = BulletDirection();
            return currentWeaponSettings.ApplySpread(direction) * _playerWeaponFireData.PlayerBulletSettingsSO.BulletSpeed;
        }

        private IEnumerator BurstFire()
        {
            _playerWeaponFireData.PlayerRigSettingsSO.IsGrabbingWeapon = true;
            var currentWeaponSettings = _playerWeaponFireData.CurrentWeaponSettingsSO;

            for (int i = 1; i <= currentWeaponSettings.BulletPerFire; i++)
            {
                FireSingleBullet();
                yield return new WaitForSeconds(currentWeaponSettings.BurstFireDelay);
            }

            _playerWeaponFireData.PlayerRigSettingsSO.IsGrabbingWeapon = false;
        }

        #endregion

        #region Animation and Effects

        private void HandleFireAnimation()
        {
            var currentWeaponSettings = _playerWeaponFireData.CurrentWeaponSettingsSO;

            if (currentWeaponSettings.WeaponAmmo <= 0)
            {
                currentWeaponSettings.IsShooting = false;
            }

            if (_canFire)
            {
                bool isShooting = currentWeaponSettings.IsShooting;

                if (isShooting)
                {
                    // Eğer Scoped özelliği varsa ve oyuncu nişan alıyorsa ScopedFire aktif olsun
                    if (_playerWeaponFireData.CurrentWeaponSettingsSO.IsWeaponScopped)
                    {
                        _playerWeaponFireData.Animator.SetBool("ScopedFire", true);
                        _scopedFireTimer = 0f;
                        _isScopedFireDelayed = false;
                    }
                    else
                    {
                        // Scoped olmayan atışta ScopedFire devreye girmesin
                        _playerWeaponFireData.Animator.SetBool("ScopedFire", false);
                    }
                }
                else
                {      
                    HandleScopedFireDelay();
                }
            }
        }

        private void HandleScopedFireDelay()
        {
            if (!_isScopedFireDelayed)
            {
                _scopedFireTimer = 0.5f;
                _isScopedFireDelayed = true;
            }

            if (_isScopedFireDelayed)
            {
                _scopedFireTimer -= Time.deltaTime;
                if (_scopedFireTimer <= 0f)
                {
                    _playerWeaponFireData.Animator.SetBool("ScopedFire", false);
                    _isScopedFireDelayed = false;
                }
            }
        }

        private void TriggerMuzzleFlash(WeaponType weaponType)
        {
            if (_weaponTypeToMuzzleFlash.TryGetValue(weaponType, out ParticleSystem muzzleFlash))
            {
                muzzleFlash.Play();
            }
        }

        #endregion

        #region Audio and Recoil

        private void PlayFireSound()
        {
            var weaponType = _playerWeaponFireData.CurrentWeaponSettingsSO.WeaponType;
            EventManager.AudioEvents.AudioWeaponFireSound?.Invoke(weaponType);
        }

        private void ApplyRecoil()
        {
            float recoilX = _playerWeaponFireData.CurrentWeaponSettingsSO.WeaponRecoilAmountX;
            float recoilY = _playerWeaponFireData.CurrentWeaponSettingsSO.WeaponRecoilAmountY;

            if (_playerWeaponFireData.CurrentWeaponSettingsSO.IsWeaponScopped)
            {
                recoilX /= 4f;
                recoilY /= 4f;
            }

            EventManager.PlayerEvents.PlayerWeaponRecoil?.Invoke(recoilX, recoilY);
        }

        private void ApplyCameraShake()
        {
            EventManager.PlayerEvents.PlayerWeaponCameraShake?.Invoke(_playerWeaponFireData.CurrentWeaponSettingsSO.CameraShakeAmount, 0.3f);
        }

        private void ApplyZoomRoutine()
        {
            if (!_playerWeaponFireData.CurrentWeaponSettingsSO.IsWeaponScopped)
                EventManager.PlayerEvents.PlayerWeaponFireZoomRoutine?.Invoke
                    (_playerWeaponFireData.CurrentWeaponSettingsSO.CameraMultiplier,
                        _playerWeaponFireData.CurrentWeaponSettingsSO.CameraDuration, _playerWeaponFireData.CurrentWeaponSettingsSO.CameraFOVDistance);
            else
                EventManager.PlayerEvents.PlayerWeaponFireZoomRoutine?.Invoke
                (_playerWeaponFireData.CurrentWeaponSettingsSO.CameraMultiplier,
                    _playerWeaponFireData.CurrentWeaponSettingsSO.CameraDuration, 40);
            
        }

        private int GetValidWeaponIndex()
        {
            int index = _playerWeaponFireData.PlayerWeaponIndexSO.WeaponIndex;
            if (index < 0 || index >= _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO.Count)
            {
                // Default to pistol by setting the current weapon type and index to 0
                _playerWeaponFireData.CurrentWeaponSettingsSO.WeaponType = WeaponType.Pistol;
                _playerWeaponFireData.PlayerWeaponIndexSO.WeaponIndex = 0;
                index = 0;
            }
            return index;
        }


        #endregion

        #region UI Updates

        private void UpdateUIAmmoCount()
        {
            int index = GetValidWeaponIndex();
            EventManager.UIEvents.UIWeaponUpdate?.Invoke(
                _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO,
                _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO[index]
            );
        }


        private void UpdateAmmoCount()
        {
            int index = GetValidWeaponIndex();
            _playerWeaponEquipAndPickUpLogic.PlayerWeaponSlotSO[index].WeaponAmmo =
                _playerWeaponFireData.CurrentWeaponSettingsSO.WeaponAmmo;
        }

        #endregion

        #region Utility Methods

        private Vector3 BulletDirection()
        {
            var currentWeaponType = GetCurrentWeaponType();
            int index = _weaponTypeToIndex[currentWeaponType];
            Vector3 direction = (_playerWeaponFireData.AimTransform.position - _playerWeaponFireData.GunPoint[index].position).normalized;

            _playerWeaponFireData.WeaponHolder.LookAt(_playerWeaponFireData.AimTransform);
            _playerWeaponFireData.GunPoint[index].LookAt(_playerWeaponFireData.AimTransform);

            return direction;
        }

        private WeaponType GetCurrentWeaponType()
        {
            return _playerWeaponFireData.CurrentWeaponSettingsSO.WeaponType;
        }

        private void SetCanFire(bool canFire)
        {
            _canFire = canFire;
        }

        #endregion
    }
}
