using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyBossLogic
{
    public class EnemyBoss_MoveState : EnemyState
    {
        private EnemyBoss _enemyBoss;
        private Vector3 _destination;
        private float _actionTimer;
        private float _timeBeforeSpeedUp = 5f;
        private bool _speedUpActive;
        
        public EnemyBoss_MoveState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyBoss = _enemyBase as EnemyBoss;
        }

        public override void Enter()
        {
            base.Enter();
            
            SpeedReset();
            
            _enemyBoss._navMeshAgent.isStopped = false;
            
            _destination = _enemyBoss.GetPatrolDestination();
            
            _enemyBoss._navMeshAgent.SetDestination(_destination);
            
            _actionTimer = _enemyBoss._actionCooldown;
        }
        
        public override void Update()
        {
            base.Update();
            
            _actionTimer -= Time.deltaTime;
            
            _enemyBoss.FaceToTarget(GetNextPatrolCorner());

            if (_enemyBoss._inBattleMode)
            {
                if (ShouldSpeedUp())
                {
                    SpeedUp();
                }
                
                Vector3 playerPosition = _enemyBoss._playerTransformValueSO.PlayerTransform;
                
                _enemyBoss._navMeshAgent.SetDestination(playerPosition);
                
                if (_actionTimer < 0)
                    PerformRandomAction();
                else if (_enemyBoss.PlayerInAttackRange())
                    _stateMachine.ChangeState(_enemyBoss._attackState);
                
            }
            else
            {
                if (Vector3.Distance(_enemyBoss.transform.position, _destination) < 0.25f)
                    _stateMachine.ChangeState(_enemyBoss._idleState);
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
        
        private void PerformRandomAction()
        {
            _actionTimer = _enemyBoss._actionCooldown;

            if (Random.Range(0, 2) == 0)
            {
                TryAbility();
            }
            else
            {
                if (_enemyBoss.CanDoJumpAttack())
                    _stateMachine.ChangeState(_enemyBoss._jumpAttackState);
                else if (_enemyBoss._bossWeaponType == BossWeaponType.Hammer)
                    TryAbility();
            }
        }

        private void SpeedReset()
        {
            _speedUpActive = false;
            
            _enemyBoss._animator.SetFloat("MoveAnimSpeedMultiplier", 1);
            _enemyBoss._animator.SetFloat("MoveAnimIndex", 0);

            _enemyBoss._navMeshAgent.speed = _enemyBoss._walkSpeed;
        }
        
        private bool ShouldSpeedUp()
        {
            if (_speedUpActive)
                return false;
            
            if (Time.time > _enemyBoss._attackState._lastTimeAttacked + _timeBeforeSpeedUp)
                return true;
            
            return false;
        }
        
        private void SpeedUp()
        {
            _speedUpActive = true;
                    
            _enemyBoss._animator.SetFloat("MoveAnimIndex", 1);
            _enemyBoss._animator.SetFloat("MoveAnimSpeedMultiplier", 1.5f);
                    
            _enemyBoss._navMeshAgent.speed = _enemyBoss._runSpeed;
        }
        
        private void TryAbility()
        {
            if (_enemyBoss.CanDoAbility())
                _stateMachine.ChangeState(_enemyBoss._abilityState);
        }
    }
}