using System;
using System.Collections.Generic;
using CoverLogic;
using EnemyLogic;
using Lean.Pool;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace EnemyRangeLogic
{
    public enum CoverPerk
    {
        Unavailable,
        CanTakeCover,
        CanTakeAndChangeCover
    }
    
    public enum UnstoppablePerk
    {
        Unavailable,
        Unstoppable
    }
    
    public enum GrenadePerk
    {
        Unavailable,
        CanThrowGrenade
    }

    /// <summary>
    /// Ana düşman menzil sınıfı: 
    /// SRP'yi güçlendirmek adına kısımları küçük metodlara parçaladık,
    /// OCP'yi göz önünde bulundurarak if/else veya enum kontrollerinde 
    /// eklenebilecek yeniliklerin tek bir yerde toplanmasını kolaylaştırmaya çalıştık.
    /// </summary>
    public class EnemyRange : Enemy
    {
        #region Fields & Inspector Variables
        
        [Header("Enemy Perk")]
        public EnemyRange_WeaponModelType _enemyRangeWeaponModelType;
        public CoverPerk _coverPerk;
        public UnstoppablePerk _unstoppablePerk;
        public GrenadePerk _grenadePerk;
        public LayerMask _whoIsAlly;
        public AudioSource _weaponFireSound;

        [Header("Advance Perk")] 
        public float _advanceSpeed;
        public float _advanceStoppingDistance;
        public float _advanceDuration = 2.5f;

        [Header("Cover System")] 
        public float _minCoverTime;
        public float _safeDistance = 10f;
        public CoverPoint _currentCover { get; private set; }
        public CoverPoint _lastCover { get; private set; }
        
        [Header("Weapon Data")]
        public Transform _weaponHolder;
        public float _shootDelay;
        [Space]
        public EnemyRange_WeaponDataSO _enemyRangeWeaponDataSO;
        public GameObject _bulletPrefab;
        public Transform _gunPoint;

        [Header("Grenade Perk")] 
        public GameObject _grenadePrefab;
        public Transform _grenadeThrowPoint;
        public float _impactPower;
        public float _timeToTarget = 1.2f;
        public float _grenadeCooldown;
        public float _explosionTimer;
        public int _granadeDamage;
        private float _lastTimeGrenadeThrow = -10f;

        [Header("Aim Details")] 
        public Transform _aim;
        public LayerMask _ignoreLayer;
        public float _slowAim = 4f;
        public float _fastAim = 20f;
        public float _aimYOffset = 1.5f;

        [SerializeField] private List<EnemyRange_WeaponDataSO> _availableWeaponDataSO;
        
        // State instances
        public EnemyRange_IdleState _idleState { get; private set; }
        public EnemyRange_MoveState _moveState { get; private set; }
        public EnemyRange_BattleState _battleState { get; private set; }
        public EnemyRange_CoverState _coverState { get; private set; }
        public EnemyRange_AdvanceState _advanceState { get; private set; }
        public EnemyRange_ThrowGranadeState _throwGranadeState { get; private set; }
        public EnemyRange_DeadState _deadState { get; private set; }
        
        #endregion

        #region Unity Built-in Methods
        
        protected override void Awake()
        {
            base.Awake();
            InitializeStates();
        }

        protected override void Start()
        {
            base.Start();

            // Aim GameObject'ini serbest bırakmak.
            _aim.parent = null;
            
            InitiliazeSpeciality();
            _stateMachine.Initialize(_idleState);
            _enemyVisuals.SetupLook();
            SetupWeaponData();
        }

        protected override void Update()
        {
            base.Update();
            
            HandleRobotVoiceIfNeeded();
            _stateMachine._currentState.Update();
        }
        
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            
            Gizmos.DrawLine(transform.position, _playerTransformValueSO.PlayerTransform);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _advanceStoppingDistance);
        }

        #endregion

        #region Initialization & States

        /// <summary>
        /// State nesnelerini oluşturur.
        /// </summary>
        private void InitializeStates()
        {
            _idleState = new EnemyRange_IdleState(this, _stateMachine, "Idle");
            _moveState = new EnemyRange_MoveState(this, _stateMachine, "Move");
            _battleState = new EnemyRange_BattleState(this, _stateMachine, "Battle");
            _coverState = new EnemyRange_CoverState(this, _stateMachine, "Cover");
            _advanceState = new EnemyRange_AdvanceState(this, _stateMachine, "Advance");
            _throwGranadeState = new EnemyRange_ThrowGranadeState(this, _stateMachine, "ThrowGranade");
            _deadState = new EnemyRange_DeadState(this, _stateMachine, "Idle");
        }
        
        /// <summary>
        /// Robot sesinin oynatılmasıyla ilgili kontrolü yapar.
        /// </summary>
        private void HandleRobotVoiceIfNeeded()
        {
            if (_stateMachine._currentState == _deadState)
            {
                StopAllRobotVoices();
            }
            else if (_stateMachine._currentState != _idleState &&
                     _stateMachine._currentState != _moveState &&
                     _stateMachine._currentState != _deadState &&
                     !_isRobotVoiceCoroutineRunning)
            {
                StartCoroutine(RobotVoiceEnumerator());
            }
        }
        
        #endregion

        #region Enemy Base Overrides

        public override void EnterBattleMode()
        {
            if (_inBattleMode) return;
            
            base.EnterBattleMode();
            
            if (CanGetCover())
                _stateMachine.ChangeState(_coverState);
            else
                _stateMachine.ChangeState(_battleState);
        }

        protected override void InitiliazeSpeciality()
        {
            // Rastgele silah seçeneği
            if (_enemyRangeWeaponModelType == EnemyRange_WeaponModelType.Random)
                ChooseRandomWeaponType();
            
            // Unstoppable perk kontrolü
            if (IsUnstoppeble())
            {
                _advanceSpeed = 1;
                _animator.SetFloat("AdvanceAnimIndex", 1);
            }
        }

        public override void Die()
        {
            base.Die();
            
            if (_stateMachine._currentState != _deadState)
                _stateMachine.ChangeState(_deadState);
        }
        
        #endregion

        #region Weapon & Shooting

        /// <summary>
        /// Tek kurşun ateşler.
        /// </summary>
        public void FireSingleBullet()
        {
            _animator.SetTrigger("Shoot");
            SpawnBulletAndMuzzleFlash();

            if (_enemyRangeWeaponModelType == EnemyRange_WeaponModelType.Shotgun)
            {
                _weaponFireSound.Play();
                return;
            }
            
            AudioSource newAudioSource = LeanPool.Spawn(_weaponFireSound, transform);
            newAudioSource.Play();
            LeanPool.Despawn(newAudioSource.gameObject, newAudioSource.clip.length);

        }

        /// <summary>
        /// Mermi ve muzzle flash objelerini oluşturur.
        /// </summary>
        private void SpawnBulletAndMuzzleFlash()
        {
            Vector3 bulletDirection = (_aim.position - _gunPoint.position).normalized;
            GameObject bullet = LeanPool.Spawn(_bulletPrefab, _gunPoint.position, Quaternion.LookRotation(_gunPoint.forward));
            GameObject muzzleFlash = LeanPool.Spawn(_enemyRangeWeaponDataSO._weaponMuzzleFlash, _gunPoint.position,
                Quaternion.LookRotation(_gunPoint.forward));

            ConfigureSpawnedBullet(bullet, bulletDirection);
            
            // Muzzle flash despawn
            LeanPool.Despawn(muzzleFlash, 1f);
        }

        /// <summary>
        /// Atılan mermiye hız, hasar ve yön gibi parametreleri uygular.
        /// </summary>
        /// <param name="bullet"></param>
        /// <param name="originalDirection"></param>
        private void ConfigureSpawnedBullet(GameObject bullet, Vector3 originalDirection)
        {
            var bulletScript = bullet.GetComponent<EnemyRange_Bullet>();
            if (bulletScript != null)
            {
                bulletScript.BulletSetup(_enemyRangeWeaponDataSO._bulletDamage);
            }

            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
            Vector3 bulletDirectionWithSpread = _enemyRangeWeaponDataSO.ApplyWeaponSpread(originalDirection);

            if (bulletRigidbody != null)
            {
                bulletRigidbody.mass = 20 / _enemyRangeWeaponDataSO._bulletSpeed;
                bulletRigidbody.velocity = bulletDirectionWithSpread * _enemyRangeWeaponDataSO._bulletSpeed;
            }
        }

        /// <summary>
        /// Silah verisini seçip GunPoint'i kurar.
        /// </summary>
        private void SetupWeaponData()
        {
            List<EnemyRange_WeaponDataSO> filteredWeaponData = new List<EnemyRange_WeaponDataSO>();
            
            foreach (EnemyRange_WeaponDataSO weaponData in _availableWeaponDataSO)
            {
                if (weaponData._enemyRangeWeaponType == _enemyRangeWeaponModelType)
                    filteredWeaponData.Add(weaponData);
            }

            if (filteredWeaponData.Count > 0)
            {
                int randomIndex = Random.Range(0, filteredWeaponData.Count);
                _enemyRangeWeaponDataSO = filteredWeaponData[randomIndex];
            }
            else
            {
                Debug.LogWarning("No weapon data found for " + _enemyRangeWeaponModelType);
            }
            
            _gunPoint = _enemyVisuals._currentWeaponModel.GetComponent<EnemyRange_WeaponModel>()._gunPoint;
        }
        
        /// <summary>
        /// Rastgele silah türü seçimi.
        /// </summary>
        private void ChooseRandomWeaponType()
        {
            List<EnemyRange_WeaponModelType> validTypes = new List<EnemyRange_WeaponModelType>();

            foreach (EnemyRange_WeaponModelType value in Enum.GetValues(typeof(EnemyRange_WeaponModelType)))
            {
                if (value != EnemyRange_WeaponModelType.Random)
                    validTypes.Add(value);
            }

            int randomIndex = Random.Range(0, validTypes.Count);
            _enemyRangeWeaponModelType = validTypes[randomIndex];
        }

        #endregion
        
        #region Grenade

        /// <summary>
        /// El bombası atma işlemi.
        /// </summary>
        public void ThrowGrenade()
        {
            if (_grenadePerk == GrenadePerk.Unavailable)
                return;
            
            _lastTimeGrenadeThrow = Time.time;
            _enemyVisuals.EnableGranedeModel(false);

            GameObject newGranede = LeanPool.Spawn(_grenadePrefab, _grenadeThrowPoint.transform.position, Quaternion.identity);
            EnemyGrenade newGrenadeScript = newGranede.GetComponent<EnemyGrenade>();

            Vector3 enemyOffsetPosition = transform.position + new Vector3(0, 0.2f, 0);
            Vector3 playerOffsetPosition = _playerTransformValueSO.PlayerTransform + new Vector3(0, 0.2f, 0);

            // Eğer düşman ölmüş durumdaysa en azından eldeki konuma göre patlasın.
            if (_stateMachine._currentState == _deadState)
            {
                newGrenadeScript.SetupGrenade(_whoIsAlly, enemyOffsetPosition, 1, _explosionTimer, _impactPower, _granadeDamage);
                return;
            }

            newGrenadeScript.SetupGrenade(_whoIsAlly, playerOffsetPosition, _timeToTarget, _explosionTimer, _impactPower, _granadeDamage);
        }
        
        /// <summary>
        /// El bombası atılabilir mi?
        /// </summary>
        /// <returns></returns>
        public bool CanThrowGrenade()
        {
            if (_grenadePerk == GrenadePerk.Unavailable)
                return false;

            if (Vector3.Distance(_playerTransformValueSO.PlayerTransform, transform.position) < _safeDistance)
                return false;

            if (Time.time > _lastTimeGrenadeThrow + _grenadeCooldown)
                return true;
            
            return false;
        }

        #endregion

        #region Cover

        /// <summary>
        /// Düşman cover alabiliyor mu?
        /// </summary>
        /// <returns></returns>
        public bool CanGetCover()
        {
            if (_coverPerk == CoverPerk.Unavailable)
                return false;

            _currentCover = AttemptToFindCover()?.GetComponent<CoverPoint>(); 
            
            if (_lastCover != _currentCover && _currentCover != null)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Bulunduğu bölgedeki en uygun cover'ı bulmaya çalışır.
        /// </summary>
        /// <returns></returns>
        private Transform AttemptToFindCover()
        {
            List<CoverPoint> collectedCoverPoints = CollectCoverPointsNearby();
            if (collectedCoverPoints.Count == 0) 
                return null;
            
            CoverPoint closestCoverPoint = FindClosestCoverPoint(collectedCoverPoints);
            if (closestCoverPoint != null)
            {
                AssignNewCover(closestCoverPoint);
                return _currentCover.transform;
            }

            return null;
        }

        /// <summary>
        /// Mevcut transform'a yakın tüm Cover objelerindeki CoverPoint'leri toplar.
        /// </summary>
        /// <returns></returns>
        private List<CoverPoint> CollectCoverPointsNearby()
        {
            float coverCollectRadius = 30;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, coverCollectRadius);
            List<Cover> collectedCovers = new List<Cover>();
            
            foreach (Collider collider in hitColliders)
            {
                Cover cover = collider.GetComponent<Cover>();
                if (cover != null && !collectedCovers.Contains(cover))
                    collectedCovers.Add(cover);
            }

            List<CoverPoint> collectedCoverPoints = new List<CoverPoint>();
            foreach (Cover cover in collectedCovers)
            {
                collectedCoverPoints.AddRange(cover.GetCoverPoints(transform));
            }

            return collectedCoverPoints;
        }

        /// <summary>
        /// En yakın cover point'i bulur.
        /// </summary>
        /// <param name="collectedCoverPoints"></param>
        /// <returns></returns>
        private CoverPoint FindClosestCoverPoint(List<CoverPoint> collectedCoverPoints)
        {
            CoverPoint closestCoverPoint = null;
            float closestDistance = float.MaxValue;
            
            foreach (CoverPoint coverPoint in collectedCoverPoints)
            {
                float distance = Vector3.Distance(transform.position, coverPoint.transform.position);
                if (distance < closestDistance)
                {
                    closestCoverPoint = coverPoint;
                    closestDistance = distance;
                }
            }

            return closestCoverPoint;
        }

        /// <summary>
        /// Yeni cover ataması yapar.
        /// </summary>
        /// <param name="closestCoverPoint"></param>
        private void AssignNewCover(CoverPoint closestCoverPoint)
        {
            _lastCover?.SetOccupied(false);
            _lastCover = _currentCover;

            _currentCover = closestCoverPoint;
            _currentCover.SetOccupied(true);
        }

        #endregion

        #region Aim & Visibility

        /// <summary>
        /// Nişan alma noktasını oyuncuya doğru günceller.
        /// </summary>
        public void UpdateAimPosition()
        {
            float aimSpeed = AimOnPlayer() ? _fastAim : _slowAim;
            Vector3 targetPosition = _playerTransformValueSO.PlayerTransform;
            targetPosition.y += _aimYOffset;
            
            _aim.position = Vector3.MoveTowards(_aim.position, targetPosition, aimSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Nişan noktası oyuncunun pozisyonuna yaklaştı mı?
        /// </summary>
        /// <returns></returns>
        public bool AimOnPlayer()
        {
            float distanceAimToPlayer = Vector3.Distance(_aim.position, _playerTransformValueSO.PlayerTransform);
            return distanceAimToPlayer < 2;
        }

        /// <summary>
        /// Oyuncuyu görebiliyor mu?
        /// </summary>
        /// <returns></returns>
        public bool IsSeesPlayer()
        {
            Vector3 myPosition = transform.position + Vector3.up;
            Vector3 playerPositionWithOffset = _playerTransformValueSO.PlayerTransform;
            playerPositionWithOffset.y += _aimYOffset;
            
            Vector3 directionToPlayer = playerPositionWithOffset - myPosition;
            
            if (Physics.Raycast(myPosition, directionToPlayer, out RaycastHit hit, Mathf.Infinity, ~_ignoreLayer))
            {
                // Raycast çarpan Transform konumunun doğrudan oyuncu ile aynı olup olmadığını kontrol ediyoruz.
                if (hit.transform.position == _playerTransformValueSO.PlayerTransform)
                {
                    UpdateAimPosition();
                    return true;
                }
            }
    
            return false;
        }

        #endregion

        #region Perks & Booleans
        
        /// <summary>
        /// Unstoppable mıyız?
        /// </summary>
        /// <returns></returns>
        public bool IsUnstoppeble()
        {
            return _unstoppablePerk == UnstoppablePerk.Unstoppable;
        }

        #endregion
    }
}
