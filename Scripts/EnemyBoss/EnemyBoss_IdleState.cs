using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyBossLogic
{
    public class EnemyBoss_IdleState : EnemyState
    {
        private EnemyBoss _enemyBoss;
        
        public EnemyBoss_IdleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyBoss = _enemyBase as EnemyBoss;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _stateTimer = _enemyBoss._idleTime;
        }

        public override void Update()
        {
            base.Update();
            
            if (_enemyBoss._inBattleMode && _enemyBoss.PlayerInAttackRange())
                _stateMachine.ChangeState(_enemyBoss._attackState);
            
            if (_stateTimer < 0)
                _stateMachine.ChangeState(_enemyBoss._moveState);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}