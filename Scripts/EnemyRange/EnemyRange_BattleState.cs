using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using PlayerScripts;
using UnityEngine;

namespace EnemyRangeLogic
{
    public class EnemyRange_BattleState : EnemyState
    {
        private EnemyRange _enemyRange;
        private float _lastTimeShoot = -10f;
        private float _weaponCooldown;
        private float _coverCheckTimer;
        private int _bulletShot = 0;
        private int _bulletPerAttack;
        private bool _firstTimeAttack = true;
        
        public EnemyRange_BattleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyRange = enemyBase as EnemyRange;
        }

        public override void Enter()
        {
            base.Enter();

            SetupValuesForFirstAttack();
            
            _enemyRange._enemyVisuals.EnableIK(true, true);
            _enemyRange._navMeshAgent.isStopped = true;
            _enemyRange._navMeshAgent.velocity = Vector3.zero;

            _stateTimer = _enemyRange._shootDelay;
        }

        public override void Update()
        {
            base.Update();
            
            if (_enemyRange.IsSeesPlayer())
                _enemyRange.FaceToTarget(_enemyRange._aim.position);
            
            if (_enemyRange.CanThrowGrenade())
                _stateMachine.ChangeState(_enemyRange._throwGranadeState);
            
            if (MustAdvanceMode())
                _stateMachine.ChangeState(_enemyRange._advanceState);
            
            ChangeCoverIfShould();
            
            if (_stateTimer > 0)
                return;
            
            if (WeaponOutOfBullet())
            {
                if (_enemyRange.IsUnstoppeble() && UnstoppebleWalk())
                {
                    _enemyRange._advanceDuration = _weaponCooldown;
                    _stateMachine.ChangeState(_enemyRange._advanceState);
                }
                
                if (WeaponOnCooldown())
                    ResetWeapon();
                
                return;
            }
            
            if (CanShoot())
            {
                Shoot();
            }
        }
        
        private void Shoot()
        {
            _enemyRange.FireSingleBullet();
            _lastTimeShoot = Time.time;
            _bulletShot++;
        }
        
        private void ResetWeapon()
        {
            _bulletShot = 0;
            
            _bulletPerAttack = _enemyRange._enemyRangeWeaponDataSO.GetRandomBulletAmount();
            _weaponCooldown = _enemyRange._enemyRangeWeaponDataSO.GetRandomWeaponCooldown();
        }
        
        private void ChangeCoverIfShould()
        {
            if (_enemyRange._coverPerk != CoverPerk.CanTakeAndChangeCover)
                return;
            
            _coverCheckTimer -= Time.deltaTime;
            
            if (_coverCheckTimer < 0)
            {
                _coverCheckTimer = 0.5f;
                
                if (ReadyToChangeCover() && ReadyToLeaveCover())
                {
                    if (_enemyRange.CanGetCover())
                        _stateMachine.ChangeState(_enemyRange._coverState);
                }
            }
        }
        
        private void SetupValuesForFirstAttack()
        {
            if (_firstTimeAttack)
            {
                //_enemyRange._agroRange = _enemyRange._advanceStoppingDistance + 2;
                
                _firstTimeAttack = false;
                
                _bulletPerAttack = _enemyRange._enemyRangeWeaponDataSO.GetRandomBulletAmount();
                _weaponCooldown = _enemyRange._enemyRangeWeaponDataSO.GetRandomWeaponCooldown();
            }
        }
        
        private bool CanShoot()
        {
            return Time.time >= _lastTimeShoot + 1 / _enemyRange._enemyRangeWeaponDataSO._fireRate;
        }

        private bool WeaponOutOfBullet()
        {
            return _bulletShot >= _bulletPerAttack;
        }

        private bool WeaponOnCooldown()
        {
            return Time.time > _lastTimeShoot + _weaponCooldown;
        }

        private bool IsPlayerInClearSight()
        {
            Vector3 directionToPlayer = _enemyRange._playerTransformValueSO.PlayerTransform - _enemyRange.transform.position;
            
            if (Physics.Raycast(_enemyRange.transform.position, directionToPlayer, out RaycastHit hit))
            {
                return hit.transform.position == _enemyRange._playerTransformValueSO.PlayerTransform;
            }
            
            return false;
        }

        private bool IsPlayerClose()
        {
            return Vector3.Distance
                (_enemyRange.transform.position, _enemyRange._playerTransformValueSO.PlayerTransform) < _enemyRange._safeDistance;
        }

        private bool ReadyToChangeCover()
        {
            bool inDanger = IsPlayerInClearSight() || IsPlayerClose();
            bool advanceTimeOver = Time.time > _enemyRange._advanceState._lastTimeAdvance + _enemyRange._advanceDuration;
            
            return inDanger && advanceTimeOver;
        }

        private bool ReadyToLeaveCover()
        {
            return Time.time > _enemyRange._minCoverTime + _enemyRange._coverState._lastTimeTookCover; 
        }
        
        private bool UnstoppebleWalk()
        {
            float distanceToPlayer = Vector3.Distance(_enemyRange.transform.position, _enemyRange._playerTransformValueSO.PlayerTransform);
            
            bool outOfStoppingDistance = distanceToPlayer > _enemyRange._advanceStoppingDistance;
            bool unstoppleWalkOnCooldown = 
                Time.time < _enemyRange._enemyRangeWeaponDataSO._minWeaponCooldown + _enemyRange._advanceState._lastTimeAdvance;
            
            return outOfStoppingDistance && unstoppleWalkOnCooldown == false;
        }

        private bool MustAdvanceMode()
        {
            if (_enemyRange.IsUnstoppeble())
                return false;

            return _enemyRange.IsPlayerInAgroRange() == false && ReadyToLeaveCover();
        }
    }
}