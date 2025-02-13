using EnemyLogic;
using EnemyStateMachineLogic;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyStateLogic
{
    public class EnemyMelee_MoveState : EnemyState
    {
        private EnemyMelee _enemyMelee;
        private Vector3 _destination;
        
        public EnemyMelee_MoveState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyMelee = enemyBase as EnemyMelee;
        }

        public override void Enter()
        {
            base.Enter();
            
            _enemyMelee._navMeshAgent.speed = _enemyMelee._walkSpeed;
            _destination = _enemyMelee.GetPatrolDestination();
            _enemyMelee._navMeshAgent.SetDestination(_destination);
        }

        public override void Update()
        {
            base.Update();
            
            _enemyMelee.FaceToTarget(GetNextPatrolCorner());
            
            if (_enemyMelee._navMeshAgent.remainingDistance <= _enemyMelee._navMeshAgent.stoppingDistance + 0.05f)
                _stateMachine.ChangeState(_enemyMelee._idleState);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}