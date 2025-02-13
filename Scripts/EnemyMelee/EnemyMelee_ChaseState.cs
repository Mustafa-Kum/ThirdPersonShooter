using EnemyLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyStateLogic
{
    public class EnemyMelee_ChaseState : EnemyState
    {
        private EnemyMelee _enemyMelee;
        private float _lastTimeUpdatedDestination;
        
        public EnemyMelee_ChaseState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyMelee = _enemyBase as EnemyMelee;
        }

        public override void Enter()
        {
            //CheckChaseAnimation();
            
            base.Enter();
            
            _enemyMelee._navMeshAgent.speed = _enemyMelee._runSpeed;
            _enemyMelee._navMeshAgent.isStopped = false;
        }

        public override void Update()
        {
            base.Update();

            if (_enemyMelee.PlayerInAttackRange())
            {
                _stateMachine.ChangeState(_enemyMelee._attackState);
            }

            _enemyMelee.FaceToTarget(_enemyMelee._playerTransformValueSO.PlayerTransform);
            
            if (CanUpdateDestination())
            {
                _enemyMelee._navMeshAgent.destination = _enemyMelee._playerTransformValueSO.PlayerTransform;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }

        private bool CanUpdateDestination()
        {
            if (Time.time > _lastTimeUpdatedDestination + 0.25f)
            {
                _lastTimeUpdatedDestination = Time.time;
                return true;
            }
            
            return false;
        }

        private void CheckChaseAnimation()
        {
            if (_enemyMelee._meleeType == Melee_Type.Shield && _enemyMelee._shieldTransform == null)
            {
                _enemyMelee._animator.SetFloat("ChaseIndex", 0);
            }
        }
    }
}