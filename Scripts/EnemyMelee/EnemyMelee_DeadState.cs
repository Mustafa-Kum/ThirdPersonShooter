using EnemyLogic;
using RagDollLogic;
using EnemyStateMachineLogic;

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
                //_enemyMelee.RagDoll.ActivateRagDollRigidBody(false); // ---> After Dead dropping the enemy to the ground
                _enemyMelee.RagDoll.ActiveRagdollCollider(false);
            }
        }
    }
}