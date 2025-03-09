using System;
using System.Collections;
using Manager;
using UnityEngine;
using UnityEngine.Serialization;

namespace CarLogic
{
    public class CarSounds : MonoBehaviour
    {
        public float _minPitch = 0.75f;
        public float _maxPitch = 1.25f;
        
        [SerializeField] private AudioSource _engineStart;
        [SerializeField] private AudioSource _engineWorking;
        [SerializeField] private AudioSource _engineOff;
        
        private CarController _carController;
        private float _maxSpeed = 10;
        private bool _allowCarSounds;

        private void Start()
        {
            _carController = GetComponent<CarController>();
            
            Invoke(nameof(AllowCarSounds), 1f);
        }
        
        private void Update()
        {
            UpdateEngineSound();
        }

        public void ActivateCarSounds(bool active)
        {
            if (_allowCarSounds == false)
                return;
            
            if (active)
            {
                StartCoroutine(PlayEngineSoundsWithDelay());
            }
            else
            {
                EventManager.AudioEvents.AudioFadeOut?.Invoke(_engineStart, 0.5f);
                EventManager.AudioEvents.AudioFadeOut?.Invoke(_engineWorking, 0.5f);
                _engineOff.Play();
            }
        }
        
        private void AllowCarSounds()
        {
            _allowCarSounds = true;
        }

        private void UpdateEngineSound()
        {
            float currentSpeed = _carController._carSpeed;
            float pitch = Mathf.Lerp(_minPitch, _maxPitch, currentSpeed / _maxSpeed);
            
            _engineWorking.pitch = pitch;
        }
        
        private IEnumerator PlayEngineSoundsWithDelay()
        {
            EventManager.AudioEvents.AudioFadeIn?.Invoke(_engineStart, 0.2f);
            
            yield return new WaitForSeconds(_engineStart.clip.length - 0.6f);
            
            EventManager.AudioEvents.AudioFadeOut?.Invoke(_engineStart, 0.5f);
            EventManager.AudioEvents.AudioFadeIn?.Invoke(_engineWorking, 0.7f);
        }
    }
}