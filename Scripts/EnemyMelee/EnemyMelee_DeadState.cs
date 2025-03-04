using EnemyLogic;
using RagDollLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyStateLogic
{
    public class EnemyMelee_DeadState : EnemyState
    {
        private EnemyMelee _enemyMelee;
        private bool _interactionDisable;
        
        public EnemyMelee_DeadState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyMelee = _enemyBase as EnemyMelee;
        }

        public override void Enter()
        {
            base.Enter();

            _enemyMelee.RagDoll.ActiveRagdollCollider(false);
            
            _interactionDisable = false;

            _stateTimer = 2f;

            UnityEngine.Object.Destroy(_enemyMelee.gameObject, 2f);
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
                _enemyMelee.RagDoll.ActivateRagDollRigidBody(false); // ---> After Dead dropping the enemy to the ground

                Debug.Log("DisableInteraction");
            }
        }
    }
}