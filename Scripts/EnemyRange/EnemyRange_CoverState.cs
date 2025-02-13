using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyRangeLogic
{
    public class EnemyRange_CoverState : EnemyState
    {
        public float _lastTimeTookCover {get; private set;}
        
        private EnemyRange _enemyRange;
        private Vector3 _destination;
        
        public EnemyRange_CoverState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyRange = enemyBase as EnemyRange;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _destination = _enemyRange._currentCover.transform.position;
            
            _enemyRange._enemyVisuals.EnableIK(true, false);
            _enemyRange._navMeshAgent.isStopped = false;
            _enemyRange._navMeshAgent.speed = _enemyRange._runSpeed;
            _enemyRange._navMeshAgent.SetDestination(_destination);
        }
        
        public override void Update()
        {
            base.Update();
            
            _enemyRange.FaceToTarget(GetNextPatrolCorner());
            
            if (Vector3.Distance(_enemyRange.transform.position, _destination) < 0.8f)
            {
                Debug.Log("Cover reached");
                _stateMachine.ChangeState(_enemyRange._battleState);
            }
        }
        
        public override void Exit()
        {
            base.Exit();
            
            _lastTimeTookCover = Time.time;
        }
    }
}