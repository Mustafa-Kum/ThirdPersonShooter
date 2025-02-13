using System.Collections.Generic;
using UnityEngine;

namespace EnemyLogic
{
    [CreateAssetMenu(fileName = "EnemyMeleeWeaponData", menuName = "Enemy/EnemyMeleeWeaponData")]
    public class EnemyMeleeWeaponData : ScriptableObject
    {
        public List<EnemyMeleeAttackData> _attackDataList;
        public float _turnSpeed;
    }
}