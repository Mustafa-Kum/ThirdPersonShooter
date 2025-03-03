using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    public enum WeaponType
    {
        Pistol,
        Revolver,
        Rifle,
        Shotgun,
        Sniper,
    }
    
    public enum ShootType
    {
        Single,
        Automatic
    }
    
    [CreateAssetMenu(fileName = "PlayerWeaponSettingsSO", menuName = "PlayerWeaponSettingsSO", order = 0)]
    public class PlayerWeaponSettingsSO : ScriptableObject
    {
        [Header("Weapon Type")]
        [SerializeField] private WeaponType _weaponType;
        [SerializeField] private ShootType _shootType;
        [SerializeField] private bool _burstModeAvailable;
        
        [Header("Weapon Ammo")]
        [SerializeField] private float _weaponAmmo;
        [SerializeField] private float _maxAmmo;
        [SerializeField] private float _totalReserveAmmo;
        [SerializeField] private float _bulletPerFire;
        
        [Header("Burst Weapon Mode")]
        [SerializeField] private float _burstModeBulletPerFire;
        [SerializeField] private float _burstModeFireRate;
        [SerializeField] private float _burstFireDelay;
        [SerializeField] private bool _burstModeActive;

        [Header("Weapon Stats")] 
        [SerializeField] private int _weaponDamage;
        [SerializeField] private float _defaultFireRate;
        [SerializeField] private float _fireRate;
        [SerializeField] private float _gunRange;
        [Range(1, 2)]
        [SerializeField] private float _reloadSpeed;
        
        [Header("Weapon Spread")]
        [SerializeField] private float _currentSpreadAmount;
        [SerializeField] private float _baseSpreadAmount;
        [SerializeField] private float _maximumSpreadAmount;
        [SerializeField] private float _spreadIncreaseAmount;
        [SerializeField] private float _spreadCooldown;
        
        [Header("Weapon Recoil")]
        [SerializeField] private float _recoilAmountX;
        [SerializeField] private float _recoilAmountY;
        
        [Header("Camera")] 
        [SerializeField] private float _cameraShakeAmount; 
        [SerializeField] private float _cameraMultiplier;
        [SerializeField] private float _cameraDuration;
        [SerializeField] private float _cameraFOVDistance; 
        
        [Header("UI Elements")]
        [SerializeField] private Sprite _weaponIcon;

        private float _lastShootTime;
        private float _lastSpreadUpdateTime;
        [SerializeField] private bool _isShooting;
        [SerializeField] private bool _isWeaponScopped;

        #region Private To Public

        public float WeaponAmmo 
        {
            get { return _weaponAmmo; }
            set { _weaponAmmo = value; }
        }

        public int WeaponDamage
        {
            get { return _weaponDamage; }
            set { _weaponDamage = value; }
        }
        
        public float MaxAmmo
        {
            get { return _maxAmmo; }
            set { _maxAmmo = value; }
        }
        
        public WeaponType WeaponType
        {
            get { return _weaponType; }
            set { _weaponType = value; }
        }
        
        public ShootType ShootType
        {
            get { return _shootType; }
            set { _shootType = value; }
        }
        
        public bool IsWeaponScopped
        {
            get { return _isWeaponScopped; }
            set { _isWeaponScopped = value; }
        }
        
        public float CameraShakeAmount
        {
            get { return _cameraShakeAmount; }
            set { _cameraShakeAmount = value; }
        }
        
        public float WeaponRecoilAmountX
        {
            get { return _recoilAmountX; }
            set { _recoilAmountX = value; }
        }
        
        public float WeaponRecoilAmountY
        {
            get { return _recoilAmountY; }
            set { _recoilAmountY = value; }
        }
        
        public float TotalReserveAmmo
        {
            get { return _totalReserveAmmo; }
            set { _totalReserveAmmo = value; }
        }
        
        public float CameraFOVDistance
        {
            get { return _cameraFOVDistance; }
            set { _cameraFOVDistance = value; }
        }
        
        public float ReloadSpeed
        {
            get { return _reloadSpeed; }
            set { _reloadSpeed = value; }
        }
        
        public bool IsShooting
        {
            get { return _isShooting; }
            set { _isShooting = value; }
        }
        
        public float BulletPerFire
        {
            get { return _bulletPerFire; }
            set { _bulletPerFire = value; }
        }
        
        public float BurstFireDelay
        {
            get { return _burstFireDelay; }
            set { _burstFireDelay = value; }
        }
        
        public float CameraDuration
        {
            get { return _cameraDuration; }
            set { _cameraDuration = value; }
        }
        
        public float CameraMultiplier
        {
            get { return _cameraMultiplier; }
            set { _cameraMultiplier = value; }
        }
        
        public float GunRange
        {
            get { return _gunRange; }
            set { _gunRange = value; }
        }

        public float CurrentSpreadAmount 
        {
            get { return _currentSpreadAmount; }
            set { _currentSpreadAmount = value; }
        }
        
        public Sprite WeaponIcon
        {
            get { return _weaponIcon; }
            set { _weaponIcon = value; }
        }

        #endregion
        
        public bool CanShoot() => _weaponAmmo > 0 && ReadyToShoot();
        
        public bool CanReload()
        {
            if (_weaponAmmo == _maxAmmo)
                return false;
            
            if (_totalReserveAmmo > 0)
            {
                return true;
            }
            
            return false;
        }

        public void ReloadAmmo()
        {
            float ammoToReload = _maxAmmo;
            
            if (ammoToReload > _totalReserveAmmo)
                ammoToReload = _totalReserveAmmo;
            
            _totalReserveAmmo -= ammoToReload;
            _weaponAmmo = ammoToReload;
            
            if (_totalReserveAmmo < 0)
                _totalReserveAmmo = 0;
        }
        
        public bool ReadyToShoot()
        {
            if (Time.time > _lastShootTime + 1 / _fireRate)
            {
                _lastShootTime = Time.time;
                return true;
            }

            return false;
        }
        
        public void ResetShootTime()
        {
            _lastShootTime = 0f;
            _lastSpreadUpdateTime = 0f;
        }

        public Vector3 ApplySpread(Vector3 originalDirection)
        {
            UpdateSpread();
    
            float randomizedValue = Random.Range(-_currentSpreadAmount, _currentSpreadAmount);
    
            Quaternion spreadRotation = Quaternion.Euler(randomizedValue, randomizedValue / 3, randomizedValue);
    
            return spreadRotation * originalDirection;
        }
        
        public void IncreaseSpread()
        {
            _currentSpreadAmount = Mathf.Clamp(_currentSpreadAmount + _spreadIncreaseAmount, _baseSpreadAmount, _maximumSpreadAmount);
            
            if (_currentSpreadAmount > _maximumSpreadAmount)
                _currentSpreadAmount = _maximumSpreadAmount;
        }
        
        public void UpdateSpread()
        {
            if (Time.time > _lastSpreadUpdateTime + _spreadCooldown)
                _currentSpreadAmount = _baseSpreadAmount;
            else
                IncreaseSpread();
            
            _lastSpreadUpdateTime = Time.time;
        }
        
        public bool BurstActivated()
        {
            if (_weaponType == WeaponType.Shotgun)
            {
                _currentSpreadAmount = 0;
                _burstFireDelay = 0;
                return true;
            }

            return _burstModeActive;
        }
        
        public void ToggleBurstMode()
        {
            if (_burstModeAvailable == false)
                return;
            
            _burstModeActive = !_burstModeActive;

            if (_burstModeActive)
            {
                _bulletPerFire = _burstModeBulletPerFire;
                _fireRate = _burstModeFireRate;
            }
            else
            {
                _bulletPerFire = 1;
                _fireRate = _defaultFireRate;
            
            }
        }
        
        public void CopyPropertiesFrom(PlayerWeaponSettingsSO source)
        {
            _weaponType = source._weaponType;
            _shootType = source._shootType;
            _weaponAmmo = source._weaponAmmo;
            _maxAmmo = source._maxAmmo;
            _totalReserveAmmo = source._totalReserveAmmo;
            _reloadSpeed = source._reloadSpeed;
            _fireRate = source._fireRate;
            _currentSpreadAmount = source._currentSpreadAmount;
            _maximumSpreadAmount = source._maximumSpreadAmount;
            _spreadIncreaseAmount = source._spreadIncreaseAmount;
            _baseSpreadAmount = source._baseSpreadAmount;
            _spreadCooldown = source._spreadCooldown;
            _bulletPerFire = source._bulletPerFire;
            _burstModeBulletPerFire = source._burstModeBulletPerFire;
            _burstModeFireRate = source._burstModeFireRate;
            _burstFireDelay = source._burstFireDelay;
            _burstModeActive = source._burstModeActive;
            _burstModeAvailable = source._burstModeAvailable;
            _defaultFireRate = source._defaultFireRate;
            _gunRange = source._gunRange;
            _weaponDamage = source._weaponDamage;
            _weaponIcon = source._weaponIcon;
            _recoilAmountX = source._recoilAmountX;
            _recoilAmountY = source._recoilAmountY;
            _cameraShakeAmount = source._cameraShakeAmount;
            _cameraMultiplier = source._cameraMultiplier;
            _cameraDuration = source._cameraDuration;
            _cameraFOVDistance = source._cameraFOVDistance;
            _isWeaponScopped = source._isWeaponScopped;
        }
    }
}