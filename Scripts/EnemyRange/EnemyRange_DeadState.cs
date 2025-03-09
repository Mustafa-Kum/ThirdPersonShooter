using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;
using Manager;

namespace EnemyRangeLogic
{
    public class EnemyRange_DeadState : EnemyState
    {
        private readonly EnemyRange _enemyRange;
        private bool _isInteractionDisabled;
        private const float InteractionDisableTime = 0f;
        private const float DestroyEnemyTime = -1f;
        private const float DeadStateDuration = 10f;
        
        public EnemyRange_DeadState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyRange = enemyBase as EnemyRange;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            if (_enemyRange._throwGranadeState._finishedThrowingGranade == false)
                _enemyRange.ThrowGrenade();

            SetLayerRecursively(_enemyRange.gameObject, LayerMask.NameToLayer("Death"));
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
            if (_stateTimer <= DestroyEnemyTime && _enemyRange.gameObject.activeSelf)
            {
                DestroyEnemy();
            }
        }

        private void DisableInteraction()
        {
            _isInteractionDisabled = true;
            //_enemyMelee.RagDoll.ActivateRagDollRigidBody(false); // İhtiyaca göre yorum kaldırılabilir.
            _enemyRange.RagDoll.ActiveRagdollCollider(false);
        }

        private void DestroyEnemy()
        {
            _enemyRange.gameObject.SetActive(false);
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