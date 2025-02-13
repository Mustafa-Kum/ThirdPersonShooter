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
        [SerializeField] private GameObject[] _blood;
        [SerializeField] private GameObject _bodyAttachedBlood;

        private PlayerHealthController _playerHealthController;
        private bool _hasSpawnedAttachedBlood = false;
        private bool _playerCanTakeDamage;

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

        public override void TakeDamage(int damage)
        {
            if (_playerCanTakeDamage == true)
            {
                int adjustedDamage = CalculateAdjustedDamage(damage);
                ApplyDamageToPlayer(adjustedDamage);
                HandleAimConstraintIfAvailable();
                TriggerBloodEffects();
            }
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

        private void TriggerBloodEffects()
        {
            SpawnBloodDecal();
            SpawnAttachedBloodOnce();
        }

        private void SpawnBloodDecal()
        {
            if (_blood == null || _blood.Length == 0)
                return;

            GameObject selectedBlood = _blood[Random.Range(0, _blood.Length)];
            Vector3 spawnPosition = transform.position + Vector3.up * 0.5f; // Example position, can be adjusted
            Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);

            var blood = LeanPool.Spawn(selectedBlood, spawnPosition, rotation);
            blood.transform.parent = null;
            LeanPool.Despawn(blood, 5f);
        }

        private void SpawnAttachedBloodOnce()
        {
            if (_hasSpawnedAttachedBlood || _bodyAttachedBlood == null)
                return;

            var attachedBlood = LeanPool.Spawn(_bodyAttachedBlood, transform);
            LeanPool.Despawn(attachedBlood, 10f);
            _hasSpawnedAttachedBlood = true;
        }
    }
}
