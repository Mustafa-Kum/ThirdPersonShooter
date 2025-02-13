using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyBossLogic
{
    public class EnemyBoss_AbilityState : EnemyState
    {
        private EnemyBoss _enemyBoss;
        
        public EnemyBoss_AbilityState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyBoss = _enemyBase as EnemyBoss;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _stateTimer = _enemyBoss._flameThrowerDuration;
            
            _enemyBoss._navMeshAgent.isStopped = true;
            _enemyBoss._navMeshAgent.velocity = Vector3.zero;
            
            _enemyBoss._bossVisuals.EnableWeaponTrail(true);
        }
        
        public override void Update()
        {
            base.Update();
            
            _enemyBoss.FaceToTarget(_enemyBoss._playerTransformValueSO.PlayerTransform);
            
            if (ShouldDisableFlameThrower())
                DisableFlameThrower();
            
            if (_triggerCalled)
                _stateMachine.ChangeState(_enemyBoss._moveState);
        }

        public override void Exit()
        {
            base.Exit();
            
            _enemyBoss.SetAbilityToCooldown();
            _enemyBoss._bossVisuals.ResetBatteries();
        }

        public override void SpecialAbilityTrigger()
        {
            base.SpecialAbilityTrigger();

            if (_enemyBoss._bossWeaponType == BossWeaponType.FlameThrower)
            {
                _enemyBoss.ActivateFlameThrower(true);
                _enemyBoss._bossVisuals.DischargeBattery();
                _enemyBoss._bossVisuals.EnableWeaponTrail(false);
            }

            if (_enemyBoss._bossWeaponType == BossWeaponType.Hammer)
            {
                _enemyBoss.ActivateHammerFx();
            }
        }
        
        public void DisableFlameThrower()
        {
            if (_enemyBoss._bossWeaponType != BossWeaponType.FlameThrower)
                return;
            
            if (_enemyBoss._flameThrowerActive == false) 
                return;
            
            _enemyBoss.ActivateFlameThrower(false);
        }
        
        private bool ShouldDisableFlameThrower()
        {
            return _stateTimer < 0;
        }
    }
}