using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyRangeLogic
{
    public class EnemyRange_IdleState : EnemyState
    {
        private EnemyRange _enemyRange;

        public EnemyRange_IdleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyRange = enemyBase as EnemyRange;
        }

        public override void Enter()
        {
            base.Enter();
            
            _enemyRange._animator.SetFloat("IdleAnimIndex", Random.Range(0, 3));
            
            _enemyRange._enemyVisuals.EnableIK(true, false);

            _stateTimer = _enemyRange._idleTime;
        }

        public override void Update()
        {
            base.Update();
            
            if ( _stateTimer <= 0 )
                _stateMachine.ChangeState(_enemyRange._moveState);
        }
        
        public override void Exit()
        {
            base.Exit();
        }
    }
}