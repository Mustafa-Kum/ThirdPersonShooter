using System.Collections;
using System.Collections.Generic;
using EnemyLogic;
using Interface;
using Lean.Pool;
using UnityEngine;
using UnityEngine.Serialization;

namespace EnemyBossLogic
{
    public enum BossWeaponType
    {
        FlameThrower,
        Hammer
    }
    
    public class EnemyBoss : Enemy
    {
        [Header("Boss Details")]
        public BossWeaponType _bossWeaponType;
        public float _attackRange;
        public float _actionCooldown = 10f;

        [Header("Jump Attack")] 
        [SerializeField] private float _upwardModifier = 10f;
        [SerializeField] private LayerMask _whatToIgnore;
        [SerializeField] private Transform _impactPoint;
        public GameObject _jumpUpParticle;
        public GameObject _jumpDownImpactParticle;
        public float _impactRadius = 2.5f;
        public float _impactPower = 5f;
        public float _travelTimeToTarget;
        public float _jumpAttackCooldown;
        public float _minJumpDistance;
        public int _jumpAttackDamage;
        
        [Header(("Ability"))]
        public float _abilityCooldown;
        public float _minAbilityDistance;
        
        [Header("FlameThrower")] 
        public ParticleSystem _flameThrower;
        public float _flameThrowerDuration;
        public float _flameDamageCooldown;
        public int _flameDamage;
        public bool _flameThrowerActive { get; private set; }

        [Header("Hammer")] 
        public GameObject _hammerFxPrefab;
        public int _hammer360Damage;
        public Transform[] _hammer360DamagePoints;
        [SerializeField] private float _hammerCheckRadius;

        [Header("Attack")] 
        [SerializeField] private Transform[] _damagePoints;
        [SerializeField] private GameObject _meleeAttackFx;
        [SerializeField] private float _attackCheckRadius;
        [SerializeField] private int _meleeAttackDamage;
        
        [Header("Sound Settings")]
        public AudioSource _impactSound;
        public AudioSource _flameThrowerSound;
        public AudioSource _jumpSound;
        public AudioSource _jumpInAirSound;
        public AudioSource _jumpImpactSound;
        
        public EnemyBoss_IdleState _idleState { get; private set; }
        public EnemyBoss_MoveState _moveState { get; private set; }
        public EnemyBoss_AttackState _attackState { get; private set; }
        public EnemyBoss_JumpAttackState _jumpAttackState { get; private set; }
        public EnemyBoss_AbilityState _abilityState { get; private set; }
        public EnemyBoss_DeadState _deadState { get; private set; }
        public EnemyBoss_Visuals _bossVisuals { get; private set; }
        
        private float _lastTimeJumpAttack;
        private float _lastTimeFlameThrower;
        
        protected override void Awake()
        {
            base.Awake();
            
            _bossVisuals = GetComponent<EnemyBoss_Visuals>();
            
            _idleState = new EnemyBoss_IdleState(this, _stateMachine, "Idle");
            _moveState = new EnemyBoss_MoveState(this, _stateMachine, "Move");
            _attackState = new EnemyBoss_AttackState(this, _stateMachine, "Attack");
            _jumpAttackState = new EnemyBoss_JumpAttackState(this, _stateMachine, "JumpAttack");
            _abilityState = new EnemyBoss_AbilityState(this, _stateMachine, "Ability");
            _deadState = new EnemyBoss_DeadState(this, _stateMachine, "Idle");
        }
        
        protected override void Start()
        {
            base.Start();
            
            _stateMachine.Initialize(_idleState);
        }
        
        protected override void Update()
        {
            base.Update();
            
            _stateMachine._currentState.Update();
            
            if (ShouldEnterBattleMode())
                EnterBattleMode();
            
            MeleeAttackCheck(_damagePoints, _attackCheckRadius, _meleeAttackFx, _meleeAttackDamage, _impactSound);
            HandleRobotVoiceIfNeeded();
        }

        public override void EnterBattleMode()
        {
            if (_inBattleMode)
                return;
            
            base.EnterBattleMode();
            
            _stateMachine.ChangeState(_moveState);
        }

        public override void Die()
        {
            base.Die();
            
            if (_stateMachine._currentState != _deadState)
                _stateMachine.ChangeState(_deadState);
        }

        public void ActivateFlameThrower(bool activate)
        {
            _flameThrowerActive = activate;
            
            if (!activate)
            {
                _flameThrower.Stop();
                
                _animator.SetTrigger("StopFlameThrower");
                return;
            }
            
            var mainModule = _flameThrower.main;
            var extraModule = _flameThrower.transform.GetChild(0).GetComponent<ParticleSystem>().main;
            
            _flameThrowerSound.Play();
            
            mainModule.duration = _flameThrowerDuration;
            extraModule.duration = _flameThrowerDuration;
            
            _flameThrower.Clear();
            _flameThrower.Play();
        }

        public void JumpImpact()
        {
            Transform impactPoint = _impactPoint;
            
            if (_impactPoint == null)
                _impactPoint = transform;
            
            MassDamage(impactPoint.position, _impactRadius, _jumpAttackDamage);
        }

        private void MassDamage(Vector3 impactPoint, float impactRadius, int damage)
        {
            HashSet<GameObject> uniqueEntities = new HashSet<GameObject>();
            
            Collider[] colliders = Physics.OverlapSphere(impactPoint, impactRadius, ~_whatToIgnore);
            
            foreach (Collider hit in colliders)
            {
                IDamagable damagable = hit.GetComponent<IDamagable>();

                if (damagable != null)
                {
                    GameObject rootEntity = hit.transform.root.gameObject;
                    
                    if (uniqueEntities.Add(rootEntity) == false)
                        continue;
                    
                    damagable.TakeDamage(damage);
                }
                
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                
                ApplyPhysicalForce(impactPoint, impactRadius, rb);
            }
        }

        private void ApplyPhysicalForce(Vector3 impactPoint, float impactRadius, Rigidbody rb)
        {
            if (rb != null)
            {
                rb.AddExplosionForce(_impactPower, impactPoint, impactRadius, _upwardModifier, ForceMode.Impulse);
            }
        }
        
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

        public void ActivateHammerFx()
        {
            StartCoroutine(SpawnHammerFxSequence());
        }

        private IEnumerator SpawnHammerFxSequence()
        {
            WaitForSeconds delay = new WaitForSeconds(0.2f);
    
            foreach (Transform damagePoint in _hammer360DamagePoints)
            {
                GameObject newFx = LeanPool.Spawn(_hammerFxPrefab, damagePoint.position, Quaternion.identity);
                LeanPool.Despawn(newFx, 2f);
                MassDamage(damagePoint.position, _hammerCheckRadius, _hammer360Damage);
        
                yield return delay;
            }
        }

        public void SetAbilityToCooldown()
        {
            _lastTimeFlameThrower = Time.time;
        }
        
        public void SetJumpAttackToCooldown()
        {
            _lastTimeJumpAttack = Time.time;
        }
        
        public bool PlayerInAttackRange()
        {
            return Vector3.Distance(transform.position, _playerTransformValueSO.PlayerTransform) <= _attackRange;
        }
        
        public bool CanDoJumpAttack()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransformValueSO.PlayerTransform);
            
            if (distanceToPlayer < _minJumpDistance)
                return false;
            
            if (Time.time > _lastTimeJumpAttack + _jumpAttackCooldown && IsPlayerInClearSight())
            {
                return true;
            }
            
            return false;
        }

        public bool IsPlayerInClearSight()
        {
            Vector3 myPosition = transform.position + new Vector3(0, 1.5f, 0);
            Vector3 playerPosition = _playerTransformValueSO.PlayerTransform + Vector3.up;
            Vector3 directionToPlayer = (playerPosition - myPosition).normalized;
            
            if (Physics.Raycast(myPosition, directionToPlayer, out RaycastHit hit, 100, ~_whatToIgnore))
            {
                return hit.transform.position == _playerTransformValueSO.PlayerTransform;
            }
            
            return false;
        }
        
        public bool CanDoAbility()
        {
            bool playerInDistance = Vector3.Distance(transform.position, _playerTransformValueSO.PlayerTransform) < _minAbilityDistance;

            if (playerInDistance == false)
                return false;
            
            if (Time.time > _lastTimeFlameThrower + _abilityCooldown)
            {
                return true;
            }
            
            return false;
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            
            Gizmos.DrawWireSphere(transform.position, _attackRange);
            
            Vector3 myPosition = transform.position + new Vector3(0, 1.5f, 0);
            Vector3 playerPosition = _playerTransformValueSO.PlayerTransform + Vector3.up;
            
            Gizmos.DrawLine(myPosition, playerPosition);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _minJumpDistance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _impactRadius);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _minAbilityDistance);

            if (_damagePoints.Length > 0)
            {
                foreach (var damagePoints in _damagePoints)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(damagePoints.position, _attackCheckRadius);
                }
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_damagePoints[0].position, _hammerCheckRadius);
            }
        }
    }
}