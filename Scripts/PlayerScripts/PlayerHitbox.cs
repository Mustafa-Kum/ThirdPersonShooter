using System;
using System.Collections;
using HitboxLogic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Lean.Pool;
using Manager;
using Random = UnityEngine.Random;

namespace PlayerScripts
{
    public class PlayerHitbox : Hitbox
    {
        [SerializeField] protected MultiAimConstraint _multiAimConstraint;

        private PlayerHealthController _playerHealthController;
        private bool _playerCanTakeDamage = true;

        protected override void Awake()
        {
            base.Awake();
            InitializePlayerHealthController();
        }

        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerCanTakeDamage += PlayerCanTakeDamage;
        }

        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerCanTakeDamage -= PlayerCanTakeDamage;
        }

        // Varsayılan TakeDamage: collision bilgisi olmadan çalışır.
        public override void TakeDamage(int damage)
        {
            if (!_playerCanTakeDamage)
                return;

            int adjustedDamage = CalculateAdjustedDamage(damage);
            ApplyDamageToPlayer(adjustedDamage);
            HandleAimConstraintIfAvailable();
        }

        private void PlayerCanTakeDamage(bool canTakeDamage)
        {
            _playerCanTakeDamage = canTakeDamage;
        }
        
        private void InitializePlayerHealthController()
        {
            _playerHealthController = GetComponentInParent<PlayerHealthController>();
        }

        private int CalculateAdjustedDamage(int damage)
        {
            return Mathf.RoundToInt(damage * _damageMultiplier);
        }

        private void ApplyDamageToPlayer(int damage)
        {
            _playerHealthController.ReduceHealth(damage);
        }

        private void HandleAimConstraintIfAvailable()
        {
            if (_multiAimConstraint == null)
                return;
            
            StartCoroutine(SmoothWeightTransition());
        }

        private IEnumerator SmoothWeightTransition()
        {
            float duration = 0.1f;
            yield return StartCoroutine(AdjustConstraintWeightOverTime(0f, 0.3f, duration));
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(AdjustConstraintWeightOverTime(0.3f, 0f, duration));
        }

        private IEnumerator AdjustConstraintWeightOverTime(float startWeight, float endWeight, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                _multiAimConstraint.weight = Mathf.Lerp(startWeight, endWeight, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _multiAimConstraint.weight = endWeight;
        }
    }
}
