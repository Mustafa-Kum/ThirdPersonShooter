﻿using System;
using UnityEngine;
using Manager;

namespace HpController
{
    public class HealthController : MonoBehaviour
    {
        public int _maxHealth;
        public int _currentHealth;
        
        private bool _isDead;

        // _isDead durumunu dışarıya açan property
        public bool IsDead => _isDead;
        
        protected virtual void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public virtual void ReduceHealth(int damage)
        {
            _currentHealth -= damage;
        }

        public virtual void IncreaseHealth()
        {
            _currentHealth++;

            if (_currentHealth > _maxHealth)
                _currentHealth = _maxHealth;
        }

        public virtual bool ShouldDie()
        {
            if (_isDead)
                return false;

            if (_currentHealth <= 0)
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