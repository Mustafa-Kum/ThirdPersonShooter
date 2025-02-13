using System.Collections.Generic;
using EnemyLogic;
using EnemyStateMachineLogic;
using UnityEngine;

namespace EnemyStateLogic
{
    public class EnemyMelee_AttackState : EnemyState
    {
        private EnemyMelee _enemyMelee;
        private Vector3 _attackDirection;
        private float _attackMoveSpeed;

        private const float MAX_ATTACK_RANGE = 50f;
        
        public EnemyMelee_AttackState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyMelee = _enemyBase as EnemyMelee;
        }

        public override void Enter()
        {
            base.Enter();
            
            _enemyMelee.UpdateAttackData();
            _enemyMelee._enemyVisuals.EnableWeaponModel(true);
            _enemyMelee._enemyVisuals.EnableWeaponTrail(true);
            
            _attackMoveSpeed = _enemyMelee._enemyMeleeAttackData._moveSpeed;
            _enemyMelee._animator.SetFloat("AttackAnimationSpeed", _enemyMelee._enemyMeleeAttackData._animationSpeed);
            _enemyMelee._animator.SetFloat("AttackIndex", _enemyMelee._enemyMeleeAttackData._attackIndex);
            _enemyMelee._animator.SetFloat("SlashAttackIndex", Random.Range(0, 4));
            
            _enemyMelee._navMeshAgent.isStopped = true;
            _enemyMelee._navMeshAgent.velocity = Vector3.zero;
            
            _attackDirection = _enemyMelee.transform.position + (_enemyMelee.transform.forward * MAX_ATTACK_RANGE);
        }

        public override void Update()
        {
            base.Update();

            if (_enemyMelee.ManualRotationActive())
            {
                _enemyMelee.FaceToTarget(_enemyMelee._playerTransformValueSO.PlayerTransform);
                _attackDirection = _enemyMelee.transform.position + (_enemyMelee.transform.forward * MAX_ATTACK_RANGE);
            }
            
            if (_enemyMelee.ManualMovementActive())
            {
                _enemyMelee.transform.position = Vector3.MoveTowards
                    (_enemyMelee.transform.position, _attackDirection, _attackMoveSpeed * Time.deltaTime);
            }

            if (_triggerCalled)
            {
                if (_enemyMelee.PlayerInAttackRange())
                    _stateMachine.ChangeState(_enemyMelee._recoveryState);
                else
                    _stateMachine.ChangeState(_enemyMelee._chaseState);
            }
        }

        public override void Exit()
        {
            base.Exit();

            SetupNextAttack();
            
            _enemyMelee._enemyVisuals.EnableWeaponTrail(false);
        }

        private void SetupNextAttack()
        {
            int recoveryIndex = PlayerClose() ? 1 : 0;
            
            _enemyMelee._animator.SetFloat("RecoveryIndex", recoveryIndex);
            
            _enemyMelee._enemyMeleeAttackData = UpdatedAttackData();
        }

        private bool PlayerClose()
        {
            if (Vector3.Distance(_enemyMelee.transform.position, _enemyMelee._playerTransformValueSO.PlayerTransform) <= 1f)
                return true;
            
            return false;
        }

        private EnemyMeleeAttackData UpdatedAttackData()
        {
            List<EnemyMeleeAttackData> validAttacks = new List<EnemyMeleeAttackData>(_enemyMelee._attackDataList);
            
            if (PlayerClose())
                validAttacks.RemoveAll(attack => attack._melee_AttackType == Melee_AttackType.ChargeAttack);
            
            int randomIndex = Random.Range(0, validAttacks.Count);
            
            return validAttacks[randomIndex];
        }
    }
}