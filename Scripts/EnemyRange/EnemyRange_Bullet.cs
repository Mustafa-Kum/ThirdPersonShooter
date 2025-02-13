using Interface;
using Lean.Pool;
using UnityEngine;

namespace EnemyRangeLogic
{
    public class EnemyRange_Bullet : MonoBehaviour
    {
        [SerializeField] private GameObject _hitEffectPrefab;
        
        private Rigidbody _rigidbody;
        private Vector3 _startPosition;
        private float _impactForce;
        private float _gunRange;
        private int _bulletDamage;
        
        #region Unity Built-in Methods
        
        /// <summary>
        /// Rigidbody referansını alır.
        /// </summary>
        protected virtual void Awake()
        {
            InitializeRigidbody();
        }

        /// <summary>
        /// Mermi menzil kontrolü yapar.
        /// </summary>
        protected virtual void Update()
        {
            CheckDistanceToDestroy();
        }

        /// <summary>
        /// Çarpma anında mermiyi yok eder, hasar uygular ve efekt üretir.
        /// </summary>
        /// <param name="collision"></param>
        protected virtual void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision);
        }

        #endregion

        #region Private Helpers
        
        /// <summary>
        /// Rigidbody referansını cache’ler.
        /// </summary>
        private void InitializeRigidbody()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Mermi başlangıç noktasından, ayarlanan menzili aştığında kendini yok eder.
        /// </summary>
        private void CheckDistanceToDestroy()
        {
            if (Vector3.Distance(_startPosition, transform.position) > _gunRange)
            {
                DespawnBullet();
            }
        }

        /// <summary>
        /// Collision anında hasar ve efekt işlemlerini yönetir.
        /// </summary>
        /// <param name="collision"></param>
        private void HandleCollision(Collision collision)
        {
            DespawnBullet(); 
            TryApplyDamage(collision.gameObject);
            CreateHitEffect(_hitEffectPrefab, collision.contacts[0].point);
        }

        /// <summary>
        /// Çarptığı objede IDamagable arayıp hasar uygular.
        /// </summary>
        /// <param name="collisionObj"></param>
        private void TryApplyDamage(GameObject collisionObj)
        {
            IDamagable damagable = collisionObj.GetComponent<IDamagable>();
            damagable?.TakeDamage(_bulletDamage);
        }

        /// <summary>
        /// Çarpma noktasında hit efekti oluşturur ve otomatik despawn uygular.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        protected virtual void CreateHitEffect(GameObject prefab, Vector3 position)
        {
            GameObject hitEffect = LeanPool.Spawn(prefab, position, Quaternion.identity);
            LeanPool.Despawn(hitEffect, 1f);
        }

        /// <summary>
        /// Mermiyi LeanPool üzerinden despawn eder.
        /// </summary>
        protected virtual void DespawnBullet()
        {
            // LeanPool bu GameObject’i tanıyor mu?
            if (LeanPool.Links.ContainsKey(gameObject))
            {
                // Evet -> normal LeanPool despawn
                LeanPool.Despawn(gameObject);
            }
            else
            {
                // Hayır -> Destroy ile yok et
                Destroy(gameObject);
                Debug.LogWarning("This bullet wasn't spawned by LeanPool, so it was destroyed normally.", this);
            }
        }

        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Mermi başlangıç değerlerini ayarlar.
        /// </summary>
        /// <param name="bulletDamage">Merminin vereceği hasar.</param>
        /// <param name="gunRange">Menzil (varsayılan 100).</param>
        /// <param name="impactForce">Çarpma kuvveti (varsayılan 100).</param>
        public void BulletSetup(int bulletDamage, float gunRange = 100, float impactForce = 100)
        {
            _impactForce = impactForce;
            _bulletDamage = bulletDamage;
            
            _startPosition = transform.position;
            _gunRange = gunRange + 1; 
        }

        #endregion
    }
}
