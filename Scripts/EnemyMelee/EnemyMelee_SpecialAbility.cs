using EnemyLogic;
using EnemyStateMachineLogic;
using Lean.Pool;
using UnityEngine;

namespace EnemyStateLogic
{
    public class EnemyMelee_SpecialAbility : EnemyState
    {
        private EnemyMelee _enemyMelee;
        private Vector3 _movementDirection;
        private float _moveSpeed;

        private const float MAX_MOVEMENT_RANGE = 20f;
        
        public EnemyMelee_SpecialAbility(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyMelee = enemyBase as EnemyMelee;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _enemyMelee._enemyVisuals.EnableWeaponModel(true);
            
            _moveSpeed = _enemyMelee._walkSpeed;
            _movementDirection = _enemyMelee.transform.position + (_enemyMelee.transform.forward * MAX_MOVEMENT_RANGE);
        }

        public override void Update()
        {
            base.Update();
            
            if (_enemyMelee.ManualRotationActive())
            {
                _enemyMelee.FaceToTarget(_enemyMelee._playerTransformValueSO.PlayerTransform);
                _movementDirection = _enemyMelee.transform.position + (_enemyMelee.transform.forward * MAX_MOVEMENT_RANGE);
            }
            
            if (_enemyMelee.ManualMovementActive())
            {
                _enemyMelee.transform.position = Vector3.MoveTowards
                    (_enemyMelee.transform.position, _movementDirection, _enemyMelee._walkSpeed * Time.deltaTime);
            }
            
            if (_triggerCalled)
            {
                _stateMachine.ChangeState(_enemyMelee._recoveryState);
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            _enemyMelee._walkSpeed = _moveSpeed;
            _enemyMelee._animator.SetFloat("RecoveryIndex", 0);
        }

        public override void SpecialAbilityTrigger()
        {
            base.SpecialAbilityTrigger();
            
            _enemyMelee.ThrowAxe();
        }
    }
}