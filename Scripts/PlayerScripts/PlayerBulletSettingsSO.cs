using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "BulletSettingsSO", menuName = "BulletSettingsSO", order = 0)]
    public class PlayerBulletSettingsSO : ScriptableObject
    {
        [SerializeField] private GameObject[] _bulletPrefabs;
        [SerializeField] private float _bulletSpeed;
        [SerializeField] private GameObject[] _hitEffect;
        [SerializeField] private float _bulletImpactForce;
        
        public GameObject[] BulletPrefabs
        {
            get { return _bulletPrefabs; }
            set { _bulletPrefabs = value; }
        }
        
        public float BulletSpeed
        {
            get { return _bulletSpeed; }
            set { _bulletSpeed = value; }
        }
        
        public GameObject[] HitEffect
        {
            get { return _hitEffect; }
            set { _hitEffect = value; }
        }
        
        public float BulletImpactForce
        {
            get { return _bulletImpactForce; }
            set { _bulletImpactForce = value; }
        }
    }
}