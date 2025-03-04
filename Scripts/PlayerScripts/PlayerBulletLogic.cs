using System;
using System.Collections.Generic;
using Data;
using EnemyLogic;
using Interface;
using Lean.Pool;
using Manager;
using ScriptableObjects;
using UnityEngine;

namespace Logic
{
    public class PlayerBulletLogic : MonoBehaviour
    {
        [SerializeField] private PlayerBulletLogicData _playerBulletLogicData;
        
        public float _impactForce;
        
        private readonly Dictionary<WeaponType, GameObject> _weaponTypeToHitEffect = new Dictionary<WeaponType, GameObject>();

        protected virtual void Awake()
        {
            InitializeHitEffects();
            InitializeRigidbody();
        }

        protected virtual void Update()
        {
            CheckBulletRange();
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision);
        }

        /// <summary>
        /// Rigidbody'yi başlatır.
        /// </summary>
        private void InitializeRigidbody()
        {
            _playerBulletLogicData.Rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Mermi menzilini kontrol eder, menzili aşarsa despawn eder.
        /// </summary>
        private void CheckBulletRange()
        {
            if (Vector3.Distance(_playerBulletLogicData.StartPosition, transform.position) > _playerBulletLogicData.GunRange)
            {
                DespawnBullet();
            }
        }

        /// <summary>
        /// Çarpışma sonrası yapılacak işlemleri yönetir.
        /// </summary>
        private void HandleCollision(Collision collision)
        {
            SpawnHitEffectForCollision(collision);
            ApplyDamageIfPossible(collision);
            //ApplyBulletImpactToEnemy(collision);
            DespawnBullet();
        }

        /// <summary>
        /// Çarpıştığı objeye hasar uygular.
        /// </summary>
        private void ApplyDamageIfPossible(Collision collision)
        {
            IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(_playerBulletLogicData.PlayerWeaponSettingsSO.WeaponDamage);
            }
        }

        /// <summary>
        /// Çarpıştığı objeye göre mermi etkisini uygular (örneğin düşmana kuvvet uygulama).
        /// </summary>
        private void ApplyBulletImpactToEnemy(Collision collision)
        {
            Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                Vector3 force = _playerBulletLogicData.Rigidbody.linearVelocity.normalized * _impactForce;
                Rigidbody hitEnemyRigidbody = collision.collider.attachedRigidbody;
                enemy.BulletImpact(force, collision.contacts[0].point, hitEnemyRigidbody);
            }
        }

        /// <summary>
        /// Hit effect'i ilgili WeaponType'a göre spawn eder.
        /// </summary>
        private void SpawnHitEffectForCollision(Collision collision)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy"))
            {
                WeaponType currentWeaponType = _playerBulletLogicData.PlayerWeaponSettingsSO.WeaponType;
                if (_weaponTypeToHitEffect.TryGetValue(currentWeaponType, out GameObject hitEffectPrefab))
                {
                    InstantiateHitEffect(hitEffectPrefab, collision.contacts[0].point);
                }
            }
        }

        protected virtual void InstantiateHitEffect(GameObject prefab, Vector3 position)
        {
            GameObject hitEffect = LeanPool.Spawn(prefab, position, Quaternion.identity);
            LeanPool.Despawn(hitEffect, 1f); // 1 saniye sonra otomatik despawn
        }

        protected virtual void DespawnBullet()
        {
            LeanPool.Despawn(gameObject);
        }

        protected virtual void InitializeHitEffects()
        {
            var hitEffects = _playerBulletLogicData.PlayerBulletSettingsSO.HitEffect;
            foreach (WeaponType weaponType in Enum.GetValues(typeof(WeaponType)))
            {
                int index = (int)weaponType;
                if (index >= 0 && index < hitEffects.Length)
                {
                    _weaponTypeToHitEffect[weaponType] = hitEffects[index];
                }
                else
                {
                    Debug.LogWarning($"Hit effect for {weaponType} is not assigned or out of range.");
                }
            }
        }

        public void BulletSetup(float gunRange = 100, float impactForce = 100)
        {
            _impactForce = impactForce;
            _playerBulletLogicData.StartPosition = transform.position;
            _playerBulletLogicData.GunRange = gunRange + 1;
        }
    }
}
