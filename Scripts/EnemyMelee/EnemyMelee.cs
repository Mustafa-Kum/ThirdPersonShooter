using System.Collections;
using System.Collections.Generic;
using EnemyStateLogic;
using Lean.Pool;
using UnityEngine;
using UnityEngine.Serialization;

namespace EnemyLogic
{
    public enum Melee_AttackType
    {
        CloseAttack,
        ChargeAttack
    }
    
    public enum Melee_Type
    {
        Regular,
        Shield,
        Dodge,
        Thrower
    }
    
    public class EnemyMelee : Enemy
    {
        #region States & Initialization

        public EnemyMelee_IdleState _idleState { get; private set; }
        public EnemyMelee_MoveState _moveState { get; private set; }
        public EnemyMelee_RecoveryState _recoveryState { get; private set; }
        public EnemyMelee_ChaseState _chaseState { get; private set; }
        public EnemyMelee_AttackState _attackState { get; private set; }
        public EnemyMelee_DeadState _deadState { get; private set; }
        public EnemyMelee_SpecialAbility _specialAbilityState { get; private set; }
        
        [Header("Enemy Settings")]
        public Melee_Type _meleeType;
        public EnemyMelee_WeaponModelType _enemyMeleeWeaponModelType;
        [SerializeField] private float _dodgeCooldown = 3f;
        [SerializeField] private float _noDodgeTreshold;

        [Header("Shield")]
        public Transform _shieldTransform;
        public int _shieldHealth;

        #endregion

        #region Axe Throw Ability

        [Header("AxeThrow Ability")]
        public GameObject _axePrefab;
        public Transform _axeThrowPoint;
        public float _axeFlySpeed;
        public float _axeChaseTimer;
        public float _axeThrowCooldown;
        public int _axeDamage;
        
        #endregion

        #region Attack Data

        [Header("Attack Data")]
        public EnemyMeleeAttackData _enemyMeleeAttackData;
        public List<EnemyMeleeAttackData> _attackDataList;
        [SerializeField] private GameObject _meleeAttackImpactFx;

        private Enemy_WeaponModel _currentWeapon;
        private float _lastDodgeTime = -3f;
        private float _lastTimeAxeThrown = -2f;
        private bool _isAttackReady;

        #endregion

        #region Unity Built-in Methods

        /// <summary>
        /// Sınıfın ilk uyanma anında base Awake’i çağırır ve State örneklerini oluşturur.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            InitializeStates();
        }
        
        /// <summary>
        /// Oyuna başlarken gereken ayarları yapar, StateMachine’i başlatır.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _stateMachine.Initialize(_idleState);
            
            InitiliazeSpeciality();
            _enemyVisuals.SetupLook();
            UpdateAttackData();
        }
        
        /// <summary>
        /// Her frame tetiklenir, hem robot sesi kontrolü hem state’in Update’i hem de melee atak kontrolü burada yapılır.
        /// </summary>
        protected override void Update()
        {
            base.Update();
            
            HandleRobotVoiceIfNeeded();
            _stateMachine._currentState.Update();

            // Melee Attack Check her frame kontrol ediliyor:
            HandleMeleeAttackCheck();
        }

        /// <summary>
        /// Gizmo çizmeyi override edip düşmanın saldırı menzilini gösterir.
        /// </summary>
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _enemyMeleeAttackData._attackRange);
        }

        #endregion

        #region Battle & Combat Overrides

        /// <summary>
        /// Düşman Battle Mode’a ilk defa girecekse RecoveryState'e sokar.
        /// </summary>
        public override void EnterBattleMode()
        {
            if (_inBattleMode)
                return;
            
            base.EnterBattleMode();
            _stateMachine.ChangeState(_recoveryState);
        }

        /// <summary>
        /// Hasar alınca dodge yapmayı da dener.
        /// </summary>
        /// <param name="damage"></param>
        public override void GetHit(int damage)
        {
            base.GetHit(damage);
            TryActiveDodge();
        }

        /// <summary>
        /// Düşman öldüğünde DeadState’e geçer.
        /// </summary>
        public override void Die()
        {
            base.Die();
            
            if (_stateMachine._currentState != _deadState)
                _stateMachine.ChangeState(_deadState);
        }

        /// <summary>
        /// Özel yetenek tetiklenince silah modelini kapatıp yavaşlatır. (Axe throw vs.)
        /// </summary>
        public override void SpecialAbilityTrigger()
        {
            base.SpecialAbilityTrigger();
            _walkSpeed *= 0.6f;
            _enemyVisuals.EnableWeaponModel(false);
        }

        #endregion

        #region Initialization & States Setup

        /// <summary>
        /// State örneklerini oluşturur.
        /// </summary>
        private void InitializeStates()
        {
            _idleState = new EnemyMelee_IdleState(this, _stateMachine, "Idle");
            _moveState = new EnemyMelee_MoveState(this, _stateMachine, "Move");
            _recoveryState = new EnemyMelee_RecoveryState(this, _stateMachine, "Recovery");
            _chaseState = new EnemyMelee_ChaseState(this, _stateMachine, "Chase");
            _attackState = new EnemyMelee_AttackState(this, _stateMachine, "Attack");
            _deadState = new EnemyMelee_DeadState(this, _stateMachine, "Idle"); // Anim placeHolder
            _specialAbilityState = new EnemyMelee_SpecialAbility(this, _stateMachine, "AxeThrow");
        }

        /// <summary>
        /// Melee özelliğine göre animasyon değerlerini, silah modelini vs. ayarlar.
        /// </summary>
        protected override void InitiliazeSpeciality()
        {
            if (_meleeType == Melee_Type.Thrower)
            {
                _enemyMeleeWeaponModelType = EnemyMelee_WeaponModelType.Throw;
            }
            else if (_meleeType == Melee_Type.Shield)
            {
                _animator.SetFloat("ChaseIndex", 1);
                _shieldTransform.gameObject.SetActive(true);
                _enemyMeleeWeaponModelType = EnemyMelee_WeaponModelType.OneHand;
            }
            else if (_meleeType == Melee_Type.Dodge)
            {
                _enemyMeleeWeaponModelType = EnemyMelee_WeaponModelType.Unarmed;
            }
        }

        #endregion

        #region Melee Attack & Weapon

        /// <summary>
        /// Silah modeline göre saldırı verilerini günceller.
        /// </summary>
        public void UpdateAttackData()
        {
            _currentWeapon = _enemyVisuals._currentWeaponModel.GetComponent<Enemy_WeaponModel>();
            if (_currentWeapon?._enemyMeleeWeaponData != null)
            {
                _attackDataList = new List<EnemyMeleeAttackData>(_currentWeapon._enemyMeleeWeaponData._attackDataList);
                _rotationSpeed = _currentWeapon._enemyMeleeWeaponData._turnSpeed;
            }
        }

        /// <summary>
        /// Her frame çağrılan melee atak kontrolü.
        /// </summary>
        private void HandleMeleeAttackCheck()
        {
            MeleeAttackCheck(
                _currentWeapon._damagePoints,
                _currentWeapon._attackRadius,
                _meleeAttackImpactFx,
                _enemyMeleeAttackData._attackDamage,
                _bulletImpactSound
            );
        }
        
        /// <summary>
        /// Oyuncu yakın dövüş menzilinde mi?
        /// </summary>
        /// <returns></returns>
        public bool PlayerInAttackRange()
        {
            return Vector3.Distance(transform.position, _playerTransformValueSO.PlayerTransform) 
                   <= _enemyMeleeAttackData._attackRange;
        }

        #endregion

        #region Axe Throw Logic

        /// <summary>
        /// Balta fırlatabilir miyiz? Thrower tipinde miyiz ve cooldown doldu mu?
        /// </summary>
        /// <returns></returns>
        public bool CanThrowAxe()
        {
            if (_meleeType != Melee_Type.Thrower)
                return false;
            
            if (Time.time >= _lastTimeAxeThrown + _axeThrowCooldown)
            {
                _lastTimeAxeThrown = Time.time;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Balta fırlatma işlemini gerçekleştirir.
        /// </summary>
        public void ThrowAxe()
        {
            GameObject newAxe = LeanPool.Spawn(_axePrefab, _axeThrowPoint.position, _axePrefab.transform.rotation);
            newAxe.GetComponent<EnemyMelee_ThrowAxe>()
                  .AxeSetup(_axeFlySpeed, _playerTransformValueSO.PlayerTransform, _axeChaseTimer, _axeDamage);
        }
        
        #endregion

        #region Dodge Logic

        /// <summary>
        /// Hasar alınca dodge yapmaya uygun olup olmadığını kontrol eder.
        /// </summary>
        private void TryActiveDodge()
        {
            float dodgeAnimDuration = GetAnimationDuration("Dodge");
            bool isHealthDropped = _enemyHealth._currentHealth < _enemyHealth._maxHealth;
            bool isDodgeCooldownOver = Time.time >= _lastDodgeTime + dodgeAnimDuration + _dodgeCooldown;
            bool isMeleeTypeDodge = _meleeType == Melee_Type.Dodge;
            bool isFarEnoughToDodge = Vector3.Distance(transform.position, _playerTransformValueSO.PlayerTransform) > _noDodgeTreshold;
            bool isChaseStateActive = _stateMachine._currentState == _chaseState;

            if (isHealthDropped && isDodgeCooldownOver && isMeleeTypeDodge && isFarEnoughToDodge && isChaseStateActive)
            {
                _animator.SetTrigger("Dodge");
                _lastDodgeTime = Time.time;
            }
        }
        
        /// <summary>
        /// Animation Controller içerisindeki klibin süresini döndürür.
        /// </summary>
        /// <param name="clipName"></param>
        /// <returns></returns>
        private float GetAnimationDuration(string clipName)
        {
            AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == clipName)
                    return clip.length;
            }
            return 0;
        }

        #endregion

        #region Audio & Voice

        /// <summary>
        /// Robot sesi durumu kontrol edilip gerekli coroutine başlatılır veya durdurulur.
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
    }

    [System.Serializable]
    public struct EnemyMeleeAttackData
    {
        public string _attackName;
        public float _attackRange;
        public float _moveSpeed;
        public float _attackIndex;
        public int _attackDamage;
        
        [Range(1, 2)]
        public float _animationSpeed;
        
        public Melee_AttackType _melee_AttackType;
    }
}
