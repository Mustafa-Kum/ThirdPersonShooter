using EnemyBossLogic;
using Lean.Pool;
using Manager;
using UnityEngine;

namespace EnemyLogic
{
    public class EnemyAnimationEvents : MonoBehaviour
    {
        private Enemy _enemy;
        private EnemyBoss _enemyBoss;
        
        private void Awake()
        {
            _enemy = GetComponentInParent<Enemy>();
            _enemyBoss = GetComponentInParent<EnemyBoss>();
        }

        public void AnimationTrigger()
        {
            _enemy.AnimationTrigger();
        }
        
        public void StartManualMovement()
        {
            _enemy.ActivateManualMovement(true);
        }
        
        public void StopManualMovement()
        {
            _enemy.ActivateManualMovement(false);
        }
        
        public void StartManualRotation()
        {
            _enemy.ActivateManualRotation(true);
        }
        
        public void StopManualRotation()
        {
            _enemy.ActivateManualRotation(false);
        }
        
        public void SpecialAbilityEvent()
        {
            _enemy.SpecialAbilityTrigger();
        }

        public void FootStepSound()
        {
            _enemy.FootStepSound();
        }
        
        public void RunStepSound()
        {
            _enemy.RunStepSound();
        }

        public void DodgeSound()
        {
            _enemy.DodgeSound();
        }

        public void JumpUpSound()
        {
            _enemyBoss._jumpSound.Play();
        }
        
        public void JumpInAirSound()
        {
            _enemyBoss._jumpInAirSound.Play();
        }
        
        public void JumpImpactSound()
        {
            _enemyBoss._jumpImpactSound.Play();
        }

        public void JumpUpParticle()
        {
            GameObject jumpUpParticle = LeanPool.Spawn(_enemyBoss._jumpUpParticle, null);
            jumpUpParticle.transform.position = _enemyBoss.transform.position;
            LeanPool.Despawn(jumpUpParticle, 2f);
        }

        public void JumpDownImpactParticle()
        {
            GameObject jumpDownImpactParticle = LeanPool.Spawn(_enemyBoss._jumpDownImpactParticle, null);
            jumpDownImpactParticle.transform.position = _enemyBoss.transform.position;
            LeanPool.Despawn(jumpDownImpactParticle, 10f);
        }
        
        public void EnableIK()
        {
            _enemy._enemyVisuals.EnableIK(true, true, 1.5f);
        }
        
        public void EnableWeaponModel()
        {
            _enemy._enemyVisuals.EnableWeaponModel(true);
            _enemy._enemyVisuals.EnableSecondaryWeaponModel(false);
        }
        
        public void BossJumpImpact()
        {
            _enemyBoss?.JumpImpact();
        }

        public void BeginMeleeAttackCheck()
        {
            _enemy?.EnableMeleeAttackCheck(true);
            EnemySwooshSound();
        }

        public void FinishMeleeAttackCheck()
        {
            _enemy?.EnableMeleeAttackCheck(false);
        }
        
        public void EnemySwooshSound()
        {
            EventManager.AudioEvents.AudioEnemyMeleeSwooshSound?.Invoke(_enemy._swooshSound, true, 0.9f, 1.1f);
        }
    }
}