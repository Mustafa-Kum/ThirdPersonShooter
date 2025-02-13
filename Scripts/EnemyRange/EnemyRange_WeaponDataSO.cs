using EnemyLogic;
using UnityEngine;

namespace EnemyRangeLogic
{
    [CreateAssetMenu(fileName = "EnemyRange_WeaponDataSO", menuName = "EnemyRange_WeaponDataSO", order = 0)]
    public class EnemyRange_WeaponDataSO : ScriptableObject
    {
        [Header("Weapon Data")] 
        public EnemyRange_WeaponModelType _enemyRangeWeaponType;
        public float _fireRate = 1f;
        public float _minWeaponCooldown = 1f;
        public float _maxWeaponCooldown = 2f;
        public GameObject _weaponMuzzleFlash;
        
        [Header("Bullet Data")]
        public float _bulletSpeed = 20f;
        public float _weaponSpread = 0.1f;
        public int _minBulletPerShot = 1;
        public int _maxBulletPerShot = 1;
        public int _bulletDamage;
        
        public int GetRandomBulletAmount()
        {
            return Random.Range(_minBulletPerShot, _maxBulletPerShot + 1);
        }
        
        public float GetRandomWeaponCooldown()
        {
            return Random.Range(_minWeaponCooldown, _maxWeaponCooldown);
        }
        
        public Vector3 ApplyWeaponSpread(Vector3 originalDirection)
        {
            float randomizeValue = Random.Range(-_weaponSpread,_weaponSpread);
            Quaternion spreadRotation = Quaternion.Euler(randomizeValue, randomizeValue, randomizeValue);
            return spreadRotation * originalDirection;
        }
    }
}