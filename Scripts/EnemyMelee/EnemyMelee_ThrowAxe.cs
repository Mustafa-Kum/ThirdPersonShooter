using Interface;
using Lean.Pool;
using ScriptableObjects;
using UnityEngine;

namespace EnemyLogic
{
    public class EnemyMelee_ThrowAxe : MonoBehaviour
    {
        [SerializeField] private PlayerTransformValueSO _playerTransformValueSO;
        [SerializeField] private Rigidbody _axeRigidbody;
        [SerializeField] private Transform _axeTransform;
        [SerializeField] private float _flySpeed;
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private GameObject _impactEffect;

        private Vector3 _axeThrowDirection;
        private float _timer = 1f;
        private int _axeDamage;

        private void Update()
        {
            _axeTransform.Rotate(Vector3.right, _rotationSpeed * Time.deltaTime);
            
            _timer -= Time.deltaTime;

            if (_timer > 0)
            {
                _axeThrowDirection = (_playerTransformValueSO.PlayerTransform + Vector3.up - _axeTransform.position);
            }
            
            transform.forward = _axeRigidbody.linearVelocity;
        }

        private void FixedUpdate()
        {
            _axeRigidbody.linearVelocity = _axeThrowDirection.normalized * _flySpeed;
        }

        public void AxeSetup(float flySpeed, Vector3 playerTransform, float timer, int damage)
        {
            _flySpeed = flySpeed;
            _playerTransformValueSO.PlayerTransform = playerTransform;
            _timer = timer;
            _axeDamage = damage;
        }

        private void OnCollisionEnter(Collision other)
        {
            IDamagable damagable = other.gameObject.GetComponent<IDamagable>();
            damagable?.TakeDamage(_axeDamage);
            
            LeanPool.Despawn(gameObject);
            GameObject impactFX = LeanPool.Spawn(_impactEffect, transform.position, Quaternion.identity);
            LeanPool.Despawn(impactFX, 2f);
        }
    }
}