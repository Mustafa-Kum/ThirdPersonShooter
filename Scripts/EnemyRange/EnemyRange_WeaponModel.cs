using System;
using EnemyLogic;
using UnityEngine;
using UnityEngine.Serialization;

namespace EnemyRangeLogic
{
    public enum EnemyRange_HoldType
    {
        Common,
        LowHold,
        HighHold
    }
    
    public class EnemyRange_WeaponModel : MonoBehaviour
    {
        public EnemyRange_WeaponModelType _enemyRangeWeaponModelType;
        public EnemyRange_HoldType _enemyRangeHoldType;

        public Transform _leftHandTarget;
        public Transform _leftElbowTarget;
        [Space]
        public Transform _gunPoint;
    }
}