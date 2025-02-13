using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyRangeLogic
{
    public class EnemyRange_MoveState : EnemyState
    {
        private EnemyRange _enemyRange;
        private Vector3 _destination;
        
        public EnemyRange_MoveState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyRange = enemyBase as EnemyRange;
        }

        public override void Enter()
        {
            base.Enter();
            
            _enemyRange._navMeshAgent.speed = _enemyRange._walkSpeed;
            _destination = _enemyRange.GetPatrolDestination();
            _enemyRange._navMeshAgent.SetDestination(_destination);
        }

        public override void Update()
        {
            base.Update();
            
            _enemyRange.FaceToTarget(GetNextPatrolCorner());
            
            if (_enemyRange._navMeshAgent.remainingDistance <= _enemyRange._navMeshAgent.stoppingDistance + 0.05f)
                _stateMachine.ChangeState(_enemyRange._idleState);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}