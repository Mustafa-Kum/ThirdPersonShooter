using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyBossLogic
{
    public class EnemyBoss_DeadState : EnemyState
    {
        private EnemyBoss _enemyBoss;
        private bool _interactionDisable;
        
        public EnemyBoss_DeadState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyBoss = enemyBase as EnemyBoss;
        }

        public override void Enter()
        {
            base.Enter();
            
            _enemyBoss._abilityState.DisableFlameThrower();
            
            _interactionDisable = false;

            _stateTimer = 2f;
        }

        public override void Update()
        {
            base.Update();
            
            DisableInteraction();
        }
        
        public override void Exit()
        {
            base.Exit();
        }
        
        private void DisableInteraction()
        {
            if (_stateTimer <= 0 && _interactionDisable == false)
            {
                _interactionDisable = true;
                //_enemyRagDoll.ActivateRagDollRigidBody(false); // ---> After Dead dropping the enemy to the ground
                _enemyBoss.RagDoll.ActiveRagdollCollider(false);
            }
        }
    }
}