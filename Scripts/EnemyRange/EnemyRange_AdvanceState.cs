using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyRangeLogic
{
    public class EnemyRange_AdvanceState : EnemyState
    {
        public float _lastTimeAdvance {get; private set;}
        
        private EnemyRange _enemyRange;
        private Vector3 _playerPosition;
        
        public EnemyRange_AdvanceState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyRange = enemyBase as EnemyRange;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _enemyRange._enemyVisuals.EnableIK(true, true);
            
            _enemyRange._navMeshAgent.isStopped = false;
            _enemyRange._navMeshAgent.speed = _enemyRange._advanceSpeed;

            if (_enemyRange.IsUnstoppeble())
            {
                _enemyRange._enemyVisuals.EnableIK(true, false);
                _stateTimer = _enemyRange._advanceDuration;
            }
        }
        
        public override void Update()
        {
            base.Update();

            _playerPosition = _enemyRange._playerTransformValueSO.PlayerTransform;
            _enemyRange.UpdateAimPosition();
            
            _enemyRange._navMeshAgent.SetDestination(_playerPosition);
            _enemyRange.FaceToTarget(GetNextPatrolCorner());
            
            if (CanEnterBattleState() && _enemyRange.IsSeesPlayer())
            {
                _stateMachine.ChangeState(_enemyRange._battleState);
            }
        }
        
        public override void Exit()
        {
            base.Exit();
            
            _lastTimeAdvance = Time.time;
        }

        private bool CanEnterBattleState()
        {
            bool closeEnoughToPlayer =
                Vector3.Distance(_enemyRange.transform.position, _playerPosition) <
                _enemyRange._advanceStoppingDistance;

            if (_enemyRange.IsUnstoppeble())
                return closeEnoughToPlayer || _stateTimer < 0;
            else
                return closeEnoughToPlayer;
        }
    }
}