using UnityEngine;
using EnemyLogic;
using EnemyStateMachineLogic;
using Manager;

namespace EnemyStateLogic
{
    public class EnemyMelee_DeadState : EnemyState
    {
        private readonly EnemyMelee _enemyMelee;
        private bool _isInteractionDisabled;
        private const float InteractionDisableTime = 0f;
        private const float DestroyEnemyTime = -1f;
        private const float DeadStateDuration = 10f;

        public EnemyMelee_DeadState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName)
            : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyMelee = enemyBase as EnemyMelee;
        }

        public override void Enter()
        {
            base.Enter();

            SetLayerRecursively(_enemyMelee.gameObject, LayerMask.NameToLayer("Death"));
            EventManager.PlayerEvents.PlayerHitEnemyCrosshairFeedBack?.Invoke(true, Logic.HitArea.Death);
            _isInteractionDisabled = false;
            _stateTimer = DeadStateDuration;
        }

        public override void Update()
        {
            base.Update();
            
            HandleInteraction();
            HandleDestruction();
        }

        private void HandleInteraction()
        {
            if (!_isInteractionDisabled && _stateTimer <= InteractionDisableTime)
            {
                DisableInteraction();
            }
        }

        private void HandleDestruction()
        {
            if (_stateTimer <= DestroyEnemyTime && _enemyMelee.gameObject.activeSelf)
            {
                DestroyEnemy();
            }
        }

        private void DisableInteraction()
        {
            _isInteractionDisabled = true;
            //_enemyMelee.RagDoll.ActivateRagDollRigidBody(false); // İhtiyaca göre yorum kaldırılabilir.
            _enemyMelee.RagDoll.ActiveRagdollCollider(false);
        }

        private void DestroyEnemy()
        {
            _enemyMelee.gameObject.SetActive(false);
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}
