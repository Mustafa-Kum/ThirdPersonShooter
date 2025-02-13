using System;
using UnityEngine;

namespace RagDollLogic
{
    public class RagDoll : MonoBehaviour
    {
        [SerializeField] private Transform _ragDollRoot;
        [SerializeField] private Collider[] _ragDollColliders;
        [SerializeField] private Rigidbody[] _ragDollRigidbodies;

        private void Awake()
        {
            _ragDollColliders = _ragDollRoot.GetComponentsInChildren<Collider>();
            _ragDollRigidbodies = _ragDollRoot.GetComponentsInChildren<Rigidbody>();
            
            ActivateRagDollRigidBody(false);

            foreach (var rigidbody in _ragDollRigidbodies)
            {
                rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
        
        public void ActivateRagDollRigidBody(bool active)
        {
            foreach (Rigidbody ragDollRigidbody in _ragDollRigidbodies)
            {
                ragDollRigidbody.isKinematic = !active;
            }
        }
        
        public void ActiveRagdollCollider(bool active)
        {
            foreach (Collider ragDollCollider in _ragDollColliders)
            {
                ragDollCollider.enabled = active;
            }
        }
    }
}