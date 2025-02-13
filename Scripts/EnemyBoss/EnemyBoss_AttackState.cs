using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyBossLogic
{
    public class EnemyBoss_AttackState : EnemyState
    {
        private EnemyBoss _enemyBoss;
        
        public float _lastTimeAttacked { get; private set; }
        
        public EnemyBoss_AttackState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyBoss = _enemyBase as EnemyBoss;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _enemyBoss._bossVisuals.EnableWeaponTrail(true);
            
            _enemyBoss._animator.SetFloat("AttackAnimIndex", Random.Range(0, 2));
            _enemyBoss._navMeshAgent.isStopped = true;

            _stateTimer = 1f;
        }
        
        public override void Update()
        {
            base.Update();
            
            if (_stateTimer > 0)
                _enemyBoss.FaceToTarget(_enemyBoss._playerTransformValueSO.PlayerTransform, 20f);

            if (_triggerCalled)
            {
                if (_enemyBoss.PlayerInAttackRange())
                    _stateMachine.ChangeState(_enemyBoss._idleState);
                else
                    _stateMachine.ChangeState(_enemyBoss._moveState);
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            _lastTimeAttacked = Time.time;
            
            _enemyBoss._bossVisuals.EnableWeaponTrail(false);
        }
    }
}