using System;
using Interface;
using UnityEngine;

namespace CarLogic
{
    public class CarDamageZone : MonoBehaviour
    {
        [SerializeField] private int _carDamage;
        [SerializeField] private float _impactForce = 150f;
        [SerializeField] private float _upwardsMultiplier = 3f;
        [SerializeField] private float _minSpeedToDamage = 3.5f;
        
        private CarController _carController;

        private void Awake()
        {
            _carController = GetComponentInParent<CarController>();
        }

        private void ApplyForce(Rigidbody rigidbody)
        {
            rigidbody.isKinematic = false;
            rigidbody.AddExplosionForce(_impactForce, transform.position, 3f, _upwardsMultiplier, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_carController._rigidbody.linearVelocity.magnitude < _minSpeedToDamage) 
                return;
            
            IDamagable damagable = other.GetComponent<IDamagable>();
            
            if (damagable == null) 
                return;

            damagable.TakeDamage(_carDamage);
            
            Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
            
            if (otherRigidbody == null) 
                return;
            
            ApplyForce(otherRigidbody);
        }
    }
}