using System;
using System.Collections;
using System.Collections.Generic;
using Interface;
using UILogic;
using UnityEngine;

namespace CarLogic
{
    public class CarHealthController : MonoBehaviour, IDamagable
    {
        public int _maxHealth = 100;
        public int _currentHealth;
        
        [Header("Explosion Settings")]
        [SerializeField] private int _explosionDamage = 350;
        [SerializeField] private ParticleSystem _fireParticles;
        [SerializeField] private ParticleSystem _explosionParticles;
        [SerializeField] private float _explosionDelay = 3f;
        [SerializeField] private float _explosionForce = 7f;
        [SerializeField] private float _explosionUpwardsModifier = 2f;
        [SerializeField] private float _explosionRadius = 3f;
        [SerializeField] private Transform _explosionPoint;

        private bool _carBroken;
        private CarController _carController;

        private void Start()
        {
            _carController = GetComponent<CarController>();
            
            _currentHealth = _maxHealth;
        }

        private void Update()
        {
            if (_fireParticles.gameObject.activeSelf)
                _fireParticles.transform.rotation = Quaternion.identity;
        }

        public void UpdateCarsHealthUI()
        {
            UI.instance._inGameUI.UpdateCarHealthUI(_currentHealth, _maxHealth);
        }
        
        public void TakeDamage(int damage)
        {
            ReduceHealth(damage);
            UpdateCarsHealthUI();
        }
        
        private void ReduceHealth(int damage)
        {
            if (_carBroken)
                return;
            
            _currentHealth -= damage;
            
            if (_currentHealth <= 0)
                CarBroke();
        }
        
        private void CarBroke()
        {
            _carBroken = true;
            
            _carController.CarBroke();
            
            _fireParticles.gameObject.SetActive(true);
            StartCoroutine(ExplosionCoroutine(_explosionDelay));
        }
        
        private void CarExploded()
        {
            _carController._rigidbody.constraints = RigidbodyConstraints.None;
            
            HashSet<GameObject> uniqueEntities = new HashSet<GameObject>();
            
            Collider[] colliders = Physics.OverlapSphere(_explosionPoint.position, _explosionRadius);
            
            foreach (Collider hit in colliders)
            {
                IDamagable damagable = hit.GetComponent<IDamagable>();
                
                if (damagable != null)
                {
                    GameObject rootEntity = hit.transform.root.gameObject;
                    
                    if (uniqueEntities.Add(rootEntity) == false)
                        continue;
                    
                    damagable.TakeDamage(_explosionDamage);
                    
                    hit.GetComponentInChildren<Rigidbody>().AddExplosionForce(
                        _explosionForce, _explosionPoint.position, _explosionRadius, _explosionUpwardsModifier, ForceMode.VelocityChange);
                }
            }
        }
        
        private IEnumerator ExplosionCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            _explosionParticles.gameObject.SetActive(true);
            _carController._rigidbody.AddExplosionForce(
                _explosionForce, _explosionPoint.position, _explosionRadius, _explosionUpwardsModifier, ForceMode.Impulse);

            CarExploded();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_explosionPoint.position, _explosionRadius);
        }
    }
}