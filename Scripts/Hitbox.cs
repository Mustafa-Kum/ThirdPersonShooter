using System;
using Interface;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace HitboxLogic
{
    public class Hitbox : MonoBehaviour, IDamagable
    {
        [SerializeField] protected float _damageMultiplier = 1f;
        
        protected virtual void Awake()
        {
            
        }

        public virtual void TakeDamage(int damage)
        {
            
        }
    }
}