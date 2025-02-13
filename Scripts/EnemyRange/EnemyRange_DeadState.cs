using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyRangeLogic
{
    public class EnemyRange_DeadState : EnemyState
    {
        private EnemyRange _enemyRange;
        private bool _interactionDisable;
        
        public EnemyRange_DeadState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyRange = enemyBase as EnemyRange;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            if (_enemyRange._throwGranadeState._finishedThrowingGranade == false)
                _enemyRange.ThrowGrenade();
            
            _interactionDisable = false;

            _stateTimer = 2f;
        }

        public override void Update()
        {
            base.Update();
            
            DisableInteraction();
        }
        
        private void DisableInteraction()
        {
            if (_stateTimer <= 0 && _interactionDisable == false)
            {
                _interactionDisable = true;
                //_enemyRagDoll.ActivateRagDollRigidBody(false); // ---> After Dead dropping the enemy to the ground
                _enemyRange.RagDoll.ActiveRagdollCollider(false);
            }
        }
    }
}