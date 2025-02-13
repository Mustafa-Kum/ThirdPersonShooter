using System;
using Interface;
using UnityEngine;

namespace EnemyBossLogic
{
    public class FlameThrow_DamageArea : MonoBehaviour
    {
        private CapsuleCollider _capsuleCollider;
        private EnemyBoss _enemyBoss;
        private float _damageCooldown;
        private float _lastTimeDamaged;
        private float _activeScaleZ = 1f; // Aktif olduğunda hedef scale değeri
        private float _inactiveScaleZ = 0f; // İnaktif olduğunda hedef scale değeri
        private float _lerpSpeed = 1f; // Lerp hızı
        private int _flameDamage;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            UpdateColliderScale();
        }

        private void OnTriggerStay(Collider other)
        {
            if (!CanDamage())
                return;

            ApplyDamage(other);
        }

        private void InitializeComponents()
        {
            _enemyBoss = GetComponentInParent<EnemyBoss>();
            if (_enemyBoss == null)
            {
                Debug.LogError("EnemyBoss component is missing on the parent GameObject.");
                return;
            }

            _damageCooldown = _enemyBoss._flameDamageCooldown;
            _flameDamage = _enemyBoss._flameDamage;

            _capsuleCollider = GetComponent<CapsuleCollider>();
            if (_capsuleCollider == null)
            {
                Debug.LogError("CapsuleCollider is missing on the GameObject.");
            }
        }

        private void UpdateColliderScale()
        {
            if (_capsuleCollider == null) return;

            float targetScaleZ = _enemyBoss._flameThrowerActive ? _activeScaleZ : _inactiveScaleZ;
            Vector3 scale = _capsuleCollider.transform.localScale;
            scale.z = Mathf.Lerp(scale.z, targetScaleZ, Time.deltaTime * _lerpSpeed);
            _capsuleCollider.transform.localScale = scale;
        }

        private bool CanDamage()
        {
            return _enemyBoss != null && _enemyBoss._flameThrowerActive && Time.time - _lastTimeDamaged >= _damageCooldown;
        }

        private void ApplyDamage(Collider other)
        {
            IDamagable damagable = other.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(_flameDamage);
                _lastTimeDamaged = Time.time;
            }
        }
    }
}
