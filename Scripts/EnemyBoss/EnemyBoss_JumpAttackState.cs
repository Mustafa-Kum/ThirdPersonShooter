using EnemyLogic;
using EnemyStateLogic;
using EnemyStateMachineLogic;
using Lean.Pool;
using UnityEngine;

namespace EnemyBossLogic
{
    public class EnemyBoss_JumpAttackState : EnemyState
    {
        private EnemyBoss _enemyBoss;
        private Vector3 _lastPlayerPosition;
        private Vector3 _startPosition;
        private Vector3 _jumpStartPosition; // Zıplama başlangıç pozisyonunu tutacak yeni değişken
        private float _jumpAttackMovementSpeed;
        private float _elapsedTime;
        private float _jumpHeight;
        private bool _jumpSoundPlayed = false;
        
        public EnemyBoss_JumpAttackState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName) : base(enemyBase, stateMachine, animationBoolName)
        {
            _enemyBoss = _enemyBase as EnemyBoss;
        }
        
        public override void Enter()
        {
            base.Enter();

            _elapsedTime = 0f;
            _startPosition = _enemyBoss.transform.position;
            _jumpStartPosition = _startPosition; // Zıplama başlangıç pozisyonunu cache et
            _jumpSoundPlayed = false; 
            _lastPlayerPosition = _enemyBoss._playerTransformValueSO.PlayerTransform;
            
            float distanceToPlayer = Vector3.Distance(_lastPlayerPosition, _startPosition);
            _jumpHeight = distanceToPlayer * 0.5f;
            
            _enemyBoss._navMeshAgent.isStopped = true;
            _enemyBoss._navMeshAgent.velocity = Vector3.zero;
            
            _enemyBoss._bossVisuals.PlaceLandingZoneParticles(_lastPlayerPosition);
            _enemyBoss._bossVisuals.EnableWeaponTrail(true);
            
            _jumpAttackMovementSpeed = distanceToPlayer / _enemyBoss._travelTimeToTarget;
            
            _enemyBoss.FaceToTarget(_lastPlayerPosition, 1000);
        }
        
        public override void Update()
        {
            base.Update();
            
            _enemyBoss._navMeshAgent.enabled = !_enemyBoss.ManualMovementActive();

            if (_enemyBoss.ManualMovementActive())
            {
                _elapsedTime += Time.deltaTime;
                float normalizedTime = _elapsedTime / _enemyBoss._travelTimeToTarget;

                if (normalizedTime <= 1f)
                {
                    Vector3 horizontalPosition = Vector3.Lerp(_startPosition, _lastPlayerPosition, normalizedTime);
                    float height = Mathf.Sin(normalizedTime * Mathf.PI) * _jumpHeight;
    
                    // Boss'un aşağı doğru hareket etmeye başladığını kontrol et
                    if (normalizedTime > 0.5f && !_enemyBoss._animator.GetBool("JumpDown"))
                    {
                        _enemyBoss._animator.SetBool("JumpDown", true);
                    }
    
                    Vector3 newPosition = horizontalPosition;
                    newPosition.y = _startPosition.y + height;
                    _enemyBoss.transform.position = newPosition;
                }
            }
            
            if (_triggerCalled)
                _stateMachine.ChangeState(_enemyBoss._moveState);
        }

        public override void Exit()
        {
            base.Exit();
            
            _enemyBoss._animator.SetBool("JumpDown", false);
            _enemyBoss.SetJumpAttackToCooldown();
            _enemyBoss._bossVisuals.EnableWeaponTrail(false);
        }
    }
}