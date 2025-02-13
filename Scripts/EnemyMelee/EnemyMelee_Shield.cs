using System;
using Interface;
using UnityEngine;

namespace EnemyLogic
{
    public class EnemyMelee_Shield : MonoBehaviour, IDamagable
    {
        [SerializeField] private int _shieldHealth;
        
        private EnemyMelee _enemyMelee;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _enemyMelee = GetComponentInParent<EnemyMelee>();
            _shieldHealth = _enemyMelee._shieldHealth;
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void ReduceShieldHealth(int damage)
        {
            _shieldHealth -= damage;
            _enemyMelee.ShieldHitSound();
            
            if (_shieldHealth <= 0)
            {
                _enemyMelee._animator.SetFloat("ChaseIndex", 0);
                
                //gameObject.SetActive(false);
                _rigidbody.isKinematic = false;
                transform.parent = null;
            }
        }

        public void TakeDamage(int damage)
        {
            ReduceShieldHealth(damage);
        }
    }
}