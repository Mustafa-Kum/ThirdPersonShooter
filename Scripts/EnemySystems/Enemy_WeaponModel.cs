using System;
using UnityEngine;

namespace EnemyLogic
{
    public class Enemy_WeaponModel : MonoBehaviour
    {
        public EnemyMelee_WeaponModelType _enemyMeleeWeaponModelType;
        public AnimatorOverrideController _animatorOverrideController;
        public EnemyMeleeWeaponData _enemyMeleeWeaponData;

        [SerializeField] private GameObject[] _trailEffect;

        [Header("Damage Attributes")] 
        public Transform[] _damagePoints;
        public float _attackRadius;
        
        public void EnableTrailEffect(bool enable)
        {
            foreach (var trailEffect in _trailEffect)
            {
                trailEffect.SetActive(enable);
            }
        }

        [ContextMenu("Assign Damage Point Transforms")]
        private void GetDamagePoints()
        {
            _damagePoints = new Transform[_trailEffect.Length];

            for (int i = 0; i < _trailEffect.Length; i++)
            {
                _damagePoints[i] = _trailEffect[i].transform;
            }
        }

        private void OnDrawGizmos()
        {
            if (_damagePoints.Length > 0)
            {
                foreach (Transform points in _damagePoints)
                {
                    Gizmos.DrawWireSphere(points.position, _attackRadius);
                }
            }
        }
    }
}