using EnemyLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyStateLogic
{
    public class EnemyMelee_IdleState : EnemyState
    {
        private EnemyMelee _enemyMelee;
        
        public EnemyMelee_IdleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyMelee = _enemyBase as EnemyMelee;
        }

        public override void Enter()
        {
            base.Enter();

            _stateTimer = _enemyBase._idleTime;
        }

        public override void Update()
        {
            base.Update();
            
            if (_enemyMelee._patrolPoints.Length <= 0)
                return;
            
            if (_stateTimer <= 0)
                _stateMachine.ChangeState(_enemyMelee._moveState);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}