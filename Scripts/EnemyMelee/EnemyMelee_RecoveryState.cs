using EnemyLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyStateLogic
{
    public class EnemyMelee_RecoveryState : EnemyState
    {
        private EnemyMelee _enemyMelee;
        
        public EnemyMelee_RecoveryState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyMelee = enemyBase as EnemyMelee;
        }

        public override void Enter()
        {
            base.Enter();
            
            _enemyMelee._navMeshAgent.isStopped = true;
        }

        public override void Update()
        {
            base.Update();

            _enemyMelee.FaceToTarget(_enemyMelee._playerTransformValueSO.PlayerTransform);

            if (_triggerCalled)
            {
                if (_enemyMelee.CanThrowAxe())
                {
                    _stateMachine.ChangeState(_enemyMelee._specialAbilityState);
                    return;
                }
                
                if (_enemyMelee.PlayerInAttackRange())
                    _stateMachine.ChangeState(_enemyMelee._attackState);
                else
                    _stateMachine.ChangeState(_enemyMelee._chaseState);
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}