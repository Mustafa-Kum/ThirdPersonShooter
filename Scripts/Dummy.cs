using System;
using Interface;
using UnityEngine;

namespace DefaultNamespace
{
    public class Dummy : MonoBehaviour, IDamagable
    {
        public int _currentHealth;
        public int _maxHealth;
        public MeshRenderer _meshRenderer;
        public Material _whiteMat;
        public Material _redMat;
        public float _refreshCooldown;

        private float _lastTimeDamaged;

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            if (Time.time > _refreshCooldown + _lastTimeDamaged || Input.GetKeyDown(KeyCode.B))
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            _currentHealth = _maxHealth;
            _meshRenderer.sharedMaterial = _whiteMat;
        }

        public void TakeDamage(int damage)
        {
            _lastTimeDamaged = Time.time;
            _currentHealth -= damage;

            if (_currentHealth < 0)
            {
                Die();
            }
        }

        private void Die()
        {
            _meshRenderer.sharedMaterial = _redMat;
        }
    }
}