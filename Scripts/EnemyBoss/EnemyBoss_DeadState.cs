using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;
using Manager;

namespace EnemyBossLogic
{
    public class EnemyBoss_DeadState : EnemyState
    {
        private readonly EnemyBoss _enemyBoss;
        private bool _isInteractionDisabled;
        private const float InteractionDisableTime = 0f;
        private const float DestroyEnemyTime = -1f;
        private const float DeadStateDuration = 10f;
        
        public EnemyBoss_DeadState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyBoss = enemyBase as EnemyBoss;
        }

        public override void Enter()
        {
            base.Enter();
            
            _enemyBoss._abilityState.DisableFlameThrower();
            
            SetLayerRecursively(_enemyBoss.gameObject, LayerMask.NameToLayer("Death"));
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
            if (_stateTimer <= DestroyEnemyTime && _enemyBoss.gameObject.activeSelf)
            {
                DestroyEnemy();
            }
        }

        private void DisableInteraction()
        {
            _isInteractionDisabled = true;
            //_enemyMelee.RagDoll.ActivateRagDollRigidBody(false); // İhtiyaca göre yorum kaldırılabilir.
            _enemyBoss.RagDoll.ActiveRagdollCollider(false);
        }

        private void DestroyEnemy()
        {
            _enemyBoss.gameObject.SetActive(false);
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