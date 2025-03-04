using System;
using Manager;
using UILogic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CarLogic
{
    public enum DriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        AllWheelDrive
    }
    
    [RequireComponent(typeof(Rigidbody))]
    
    public class CarController : MonoBehaviour
    {
        [Header("Car Settings")]
        public float _carSpeed;
        [SerializeField] private LayerMask _whatIsGround;
        [SerializeField] private DriveType _driveType;
        [SerializeField] private Transform _centerOfMass;
        [Range(350, 1000)] [SerializeField] private float _carMass = 400f;
        [Range(20, 80f)] [SerializeField] private float _wheelsMass = 35f;
        [Range(0.5f, 2f)] [SerializeField] private float _frontWheelTraction = 1f;
        [Range(0.5f, 2f)] [SerializeField] private float _backWheelTraction = 1f;
        
        [Header("Engine Settings")]
        [SerializeField] private float _currentSpeed;
        [Range(1500, 5000)] [SerializeField] private float _motorForce = 1500f;
        [Range(7, 12)] [SerializeField] private float _maxSpeed = 7f;
        [Range(0.5f, 10)] [SerializeField] private float _accelerationSpeed = 2f;

        [Header("Steering Settings")] 
        [Range(30, 60)] [SerializeField] private float turnSensitivity = 30;
        
        [Header("Brake Settings")]
        [Range(0, 10)] [SerializeField] private float _frontBrakeSensitivity = 5;
        [Range(0, 10)] [SerializeField] private float _backBrakeSensitivity = 5;
        [Range(4000, 6000)] [SerializeField] private float _brakeForce = 5000f;

        [Header("Drift Settings")] 
        [SerializeField] private float _driftDuration = 1f;
        [Range(0, 1)][SerializeField] private float _frontDriftFactor = 0.5f;
        [Range(0, 1)][SerializeField] private float _backDriftFactor = 0.5f;
        
        public Rigidbody _rigidbody { get; private set; }
        public CarSounds _carSounds { get; private set; }
        
        private PlayerController _playerController;
        private CarWheel[] _carWheels;
        private UI _ui;
        private float _moveInput;
        private float _steerInput;
        private float _driftTimer;
        private bool _isBraking;
        private bool _isDrifting;
        private bool _canEmitTrail = true;
        
        public bool _carActive { get; private set; }
        
        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _carWheels = GetComponentsInChildren<CarWheel>();
            _carSounds = GetComponent<CarSounds>();
            _ui = UI.instance;
            
            _playerController = ControlsManager.instance._playerControllerData._playerController;
            //ControlsManager.instance.SwitchToCarControls();
            
            ActivateCar(false);
            AssignInputEvents();
            SetupDefaultValues();
        }

        private void Update()
        {
            if (_carActive == false)
                return;
            
            _carSpeed = _rigidbody.linearVelocity.magnitude;
            _ui._inGameUI.UpdateCarSpeedTextUI(Mathf.RoundToInt(_carSpeed * 10f) + "KM / H");
            _driftTimer -= Time.deltaTime;
            
            if (_driftTimer < 0)
                _isDrifting = false;
        }

        private void FixedUpdate()
        {
            if (_carActive == false)
                return;
            
            ApplyWheelTrails();
            ApplyAnimationToWheels();
            ApplyDrive();
            ApplySteering();
            ApplyBrake();
            ApplySpeedLimit();
            
            if (_isDrifting)
                ApplyDrift();
            else
                StopDrift();
        }
        
        [ContextMenu("Focus camera and enable")]
        public void TestThisCar()
        {
            ActivateCar(true);
        }
        
        public void ActivateCar(bool activate)
        {
            _carActive = activate;
            
            if (_carSounds != null)
                _carSounds.ActivateCarSounds(activate);

            _rigidbody.constraints = activate ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;
        }
        
        public void CarBroke()
        {
            _canEmitTrail = false;

            foreach (var wheel in _carWheels)
            {
                wheel._trailRenderer.emitting = false;
            }
            
            _rigidbody.linearDamping = 1f;
            _motorForce = 0;
            _isDrifting = true;
            _frontDriftFactor = 0.9f;
            _backDriftFactor = 0.9f;
        }

        private void ApplyWheelTrails()
        {
            if (_canEmitTrail == false)
                return;
            
            foreach (var wheel in _carWheels)
            {
                WheelHit hit;
                
                if (wheel._wheelCollider.GetGroundHit(out hit))
                {
                    if (_whatIsGround == (_whatIsGround | (1 << hit.collider.gameObject.layer)))
                    {
                        wheel._trailRenderer.emitting = true;
                    }
                    else
                    {
                        wheel._trailRenderer.emitting = false;
                    }
                }
                else
                {
                    wheel._trailRenderer.emitting = false;
                }
            }
        }

        private void ApplyDrive()
        {
            _currentSpeed = _moveInput * _accelerationSpeed * Time.deltaTime;
            
            float motorTorqueValue = _motorForce * _currentSpeed;
            
            foreach (var wheel in _carWheels)
            {
                if (_driveType == DriveType.FrontWheelDrive)
                {
                    if (wheel._axelType == AxelType.Front)
                    {
                        wheel._wheelCollider.motorTorque = motorTorqueValue;
                    }
                }
                else if (_driveType == DriveType.RearWheelDrive)
                {
                    if (wheel._axelType == AxelType.Back)
                    {
                        wheel._wheelCollider.motorTorque = motorTorqueValue;
                    }
                }
                else
                {
                    wheel._wheelCollider.motorTorque = motorTorqueValue;
                }
                
            }
        }

        private void ApplySteering()
        {
            foreach (var wheel in _carWheels)
            {
                if (wheel._axelType == AxelType.Front)
                {
                    float targetSteerAngle = turnSensitivity * _steerInput;
                    wheel._wheelCollider.steerAngle = Mathf.Lerp(wheel._wheelCollider.steerAngle, targetSteerAngle, 0.5f); 
                }
            }
        }
        
        private void ApplyBrake()
        {
            foreach (var wheel in _carWheels)
            {
                bool frontBreaks = wheel._axelType == AxelType.Front;
                float brakeSensitivity = frontBreaks ? _frontBrakeSensitivity : _backBrakeSensitivity;
                
                float newBrakeTorque = _brakeForce * brakeSensitivity * Time.deltaTime;
                float currentBrakeTorque = _isBraking ? newBrakeTorque : 0;
                
                wheel._wheelCollider.brakeTorque = currentBrakeTorque;
            }
        }

        private void ApplyDrift()
        {
            foreach (var wheels in _carWheels)
            {
                bool frontWheels = wheels._axelType == AxelType.Front;
                float driftFactor = frontWheels ? _frontDriftFactor : _backDriftFactor;
                
                WheelFrictionCurve sidewaysFriction = wheels._wheelCollider.sidewaysFriction;
                
                sidewaysFriction.stiffness *= (1 - driftFactor);
                wheels._wheelCollider.sidewaysFriction = sidewaysFriction;
            }
        }

        private void StopDrift()
        {
            foreach (var wheels in _carWheels)
            {
                wheels.RestoreDefaultStiffness();
            }
        }
        
        private void ApplySpeedLimit()
        {
            if (_rigidbody.linearVelocity.magnitude > _maxSpeed)
                _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _maxSpeed;
        }
        
        private void ApplyAnimationToWheels()
        {
            foreach (var wheel in _carWheels)
            {
                Quaternion wheelRotation;
                Vector3 wheelPosition;
                wheel._wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);
                
                if (wheel._wheelMesh != null)
                {
                    wheel._wheelMesh.transform.position = wheelPosition;
                    wheel._wheelMesh.transform.rotation = wheelRotation;
                }
                
                wheel._wheelMesh.transform.position = wheelPosition;
                wheel._wheelMesh.transform.rotation = wheelRotation;
            }
        }
        
        private void SetupDefaultValues()
        {
            _rigidbody.centerOfMass = _centerOfMass.localPosition;
            _rigidbody.mass = _carMass;

            foreach (var wheels in _carWheels)
            {
                wheels._wheelCollider.mass = _wheelsMass;
                
                if (wheels._axelType == AxelType.Front)
                    wheels.SetDefaultStiffness(_frontWheelTraction);
                
                if (wheels._axelType == AxelType.Back)
                    wheels.SetDefaultStiffness(_backWheelTraction);
            }
        }
        
        private void AssignInputEvents()
        {
            _playerController.Car.CarMovement.performed += ctx =>
            {
                Vector2 movementInput = ctx.ReadValue<Vector2>();

                _moveInput = movementInput.y;
                _steerInput = movementInput.x;
            };
            
            _playerController.Car.CarMovement.canceled += ctx =>
            {
                _moveInput = 0;
                _steerInput = 0;
            };
            
            _playerController.Car.CarBrake.performed += ctx =>
            {
                _isBraking = true;
                _isDrifting = true;
                _driftTimer = _driftDuration;
            };
            _playerController.Car.CarBrake.canceled += ctx => _isBraking = false;
            
            _playerController.Car.CarExit.performed += ctx => GetComponent<CarInteraction>().GetOutOfTheCar();
        }
    }
}