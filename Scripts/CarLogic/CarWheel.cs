using System;
using UnityEngine;

namespace CarLogic
{
    public enum AxelType
    {
        Front,
        Back
    }
    
    [RequireComponent(typeof(WheelCollider))]
    public class CarWheel : MonoBehaviour
    {
        public AxelType _axelType;
        
        public WheelCollider _wheelCollider { get; private set; }
        public GameObject _wheelMesh { get; private set; }
        public TrailRenderer _trailRenderer { get; private set; }
        
        private float _defaultSideStiffness;

        private void Awake()
        {
            _wheelCollider = GetComponent<WheelCollider>();
            _wheelMesh = GetComponentInChildren<MeshRenderer>().gameObject;
            _trailRenderer = GetComponentInChildren<TrailRenderer>();
            
            _trailRenderer.emitting = false;
        }
        
        public void SetDefaultStiffness(float newValue)
        {
            _defaultSideStiffness = newValue;
            RestoreDefaultStiffness();
        }
        
        public void RestoreDefaultStiffness()
        {
            WheelFrictionCurve sidewaysFriction = _wheelCollider.sidewaysFriction;
            
            sidewaysFriction.stiffness = _defaultSideStiffness;
            _wheelCollider.sidewaysFriction = sidewaysFriction;
        }
    }
}