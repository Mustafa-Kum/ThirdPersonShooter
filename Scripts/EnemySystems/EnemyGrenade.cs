using System.Collections.Generic;
using Interface;
using Lean.Pool;
using Manager;
using UnityEngine;

namespace EnemyLogic
{
    public class EnemyGrenade : MonoBehaviour
    {
        [SerializeField] private GameObject _explosionFx;
        [SerializeField] private float _impactRadius;
        [SerializeField] private float _upwardModifier;
        [SerializeField] private AudioSource _enemyGrenadeBounceSound;
        
        private LayerMask _allyLayerMask;
        private Rigidbody _rigidbody;
        private float _timer;
        private float _impactPower;
        private int _granadeDamage;
        private bool _canExplode = true;
        private bool _hasCollided = false;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            
            if (_timer < 0 && _canExplode)
            {
                Explode();
            }
        }

        public void SetupGrenade(LayerMask allyLayer, Vector3 target, float timeToTarget, float countdown, float impactPower, int granadeDamage)
        {
            _canExplode = true;
            _allyLayerMask = allyLayer;
            _rigidbody.velocity = CalculateThrowVelocity(target, timeToTarget);
            _timer = countdown + timeToTarget;
            _impactPower = impactPower;
            _granadeDamage = granadeDamage;
        }

        private void Explode()
        {
            _canExplode = false;
            
            PlayExplosionFx();

            HashSet<GameObject> uniqueEntities = new HashSet<GameObject>();

            Collider[] colliders = Physics.OverlapSphere(transform.position, _impactRadius);
            
            foreach (Collider hit in colliders)
            {
                IDamagable damagable = hit.GetComponent<IDamagable>();

                if (damagable != null)
                {
                    if (IsTargetValid(hit))
                    {
                        GameObject rootEntity = hit.transform.root.gameObject;
                        
                        if (uniqueEntities.Add(rootEntity) == false)
                            continue;   
                        
                        damagable.TakeDamage(_granadeDamage);
                    }
                }
                
                ApplyPhysicalForce(hit);
            }
        }

        private void ApplyPhysicalForce(Collider hit)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(_impactPower, transform.position, _impactRadius, _upwardModifier, ForceMode.Impulse);
            }
        }

        private void PlayExplosionFx()
        {
            GameObject newFx = LeanPool.Spawn(_explosionFx, transform.position, Quaternion.identity);
            EventManager.PlayerEvents.PlayerCameraShake?.Invoke(0.3f, 1f, 1f);
            
            LeanPool.Despawn(newFx, 2f);
            LeanPool.Despawn(gameObject);
        }

        private Vector3 CalculateThrowVelocity(Vector3 target, float timeToTarget)
        {
            Vector3 direction = target - transform.position;
            Vector3 directionXZ = new Vector3(direction.x, 0, direction.z);
            Vector3 velocityXZ = directionXZ / timeToTarget;
            
            float velocityY = 
                (direction.y - (Physics.gravity.y * Mathf.Pow(timeToTarget, 2)) / 2 ) / timeToTarget;
            
            Vector3 throwVelocity = velocityXZ + Vector3.up * velocityY;
            
            return throwVelocity;
        }
        
        private bool IsTargetValid(Collider collider)
        {
            if ((_allyLayerMask.value & (1 << collider.gameObject.layer)) > 0)
            {
                return false;
            }

            return true;
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            // Eğer henüz çarpmadıysa
            if (!_hasCollided)
            {
                _hasCollided = true; // Artık çarptı

                AudioSource newAudioSource = Lean.Pool.LeanPool.Spawn(_enemyGrenadeBounceSound, transform);
                newAudioSource.Play();
            
                Lean.Pool.LeanPool.Despawn(newAudioSource.gameObject, newAudioSource.clip.length);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            // Nesne yüzeyle temasını kestiğinde yeniden çarpabilir
            _hasCollided = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _impactRadius);
        }
    }
}
