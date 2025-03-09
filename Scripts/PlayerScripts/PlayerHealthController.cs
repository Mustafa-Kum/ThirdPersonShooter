using HpController;
using Manager;
using RagDollLogic;
using UILogic;
using UnityEngine;
using ScriptableObjects;

namespace PlayerScripts
{
    public class PlayerHealthController : HealthController
    {
        [SerializeField] private PlayerHealthDataSO _playerHealthDataSO;
        
        private RagDoll _ragDoll;
        private Animator _animator;

        public bool _isDead { get; private set; }
        
        protected override void Awake()
        {
            _playerHealthDataSO._currentHealthAmount = _maxHealth;

            InitializeRagdoll();
            InitializeAnimator();
        }

        public override void ReduceHealth(int damage)
        {
            _playerHealthDataSO._currentHealthAmount -= damage;
            
            CheckDeathCondition();
            UpdateHealthUI();
        }

        /// <summary>
        /// Ragdoll bileşenini bulur.
        /// </summary>
        private void InitializeRagdoll()
        {
            _ragDoll = GetComponent<RagDoll>();
        }

        /// <summary>
        /// Animator bileşenini bulur.
        /// </summary>
        private void InitializeAnimator()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        /// <summary>
        /// Ölüm kontrolünü yapar.
        /// </summary>
        private void CheckDeathCondition()
        {
            if (ShouldDie())
            {
                Die();
            }
        }

        /// <summary>
        /// Can bilgilerini UI üzerinde günceller.
        /// </summary>
        private void UpdateHealthUI()
        {
            UI.instance._inGameUI.UpdateHealthUI(_playerHealthDataSO._currentHealthAmount, _maxHealth);
        }

        /// <summary>
        /// Oyuncuyu öldürür ve ilgili işlemleri başlatır.
        /// </summary>
        private void Die()
        {
            if (_isDead)
                return;

            _isDead = true;
            ActivateRagdoll();
            DisableAnimator();
            TriggerGameOverEvent();
        }

        /// <summary>
        /// Ragdoll'u aktifleştirir.
        /// </summary>
        private void ActivateRagdoll()
        {
            _ragDoll.ActivateRagDollRigidBody(true);
        }

        /// <summary>
        /// Animatörü devre dışı bırakır.
        /// </summary>
        private void DisableAnimator()
        {
            _animator.enabled = false;
        }

        /// <summary>
        /// Game Over eventini tetikler.
        /// </summary>
        private void TriggerGameOverEvent()
        {
            EventManager.GameEvents.GameOver?.Invoke();
        }

        public override bool ShouldDie()
        {
            if (_isDead)
                return false;

            if (_playerHealthDataSO._currentHealthAmount <= 0)
            {
                _isDead = true;
                EventManager.EnemySpawnEvents.EnemyDied?.Invoke(gameObject);
                EventManager.AudioEvents.AudioKillSound?.Invoke();
                return true;
            }

            return false;
        }
    }
}