using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyRangeLogic
{
    public class EnemyRange_ThrowGranadeState : EnemyState
    {
        public bool _finishedThrowingGranade { get; private set; } = true;
        
        private EnemyRange _enemyRange;
        
        public EnemyRange_ThrowGranadeState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyRange = enemyBase as EnemyRange;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _finishedThrowingGranade = false;
            
            _enemyRange._enemyVisuals.EnableWeaponModel(false);
            _enemyRange._enemyVisuals.EnableIK(false, false);
            _enemyRange._enemyVisuals.EnableSecondaryWeaponModel(true);
            _enemyRange._enemyVisuals.EnableGranedeModel(true);
        }
        
        public override void Update()
        {
            base.Update();

            Vector3 playerPosition = _enemyRange._playerTransformValueSO.PlayerTransform + Vector3.up;
            
            _enemyRange.FaceToTarget(playerPosition);
            _enemyRange._aim.position = playerPosition;
            
            if (_triggerCalled)
                _stateMachine.ChangeState(_enemyRange._battleState);
        }

        public override void SpecialAbilityTrigger()
        {
            base.SpecialAbilityTrigger();
            
            _finishedThrowingGranade = true;
            _enemyRange.ThrowGrenade();
        }
    }
}