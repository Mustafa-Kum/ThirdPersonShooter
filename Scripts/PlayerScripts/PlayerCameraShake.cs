using System;
using Cinemachine;
using Manager;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerCameraShake : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _thirdPersonCamera;

        private CinemachineBasicMultiChannelPerlin _cameraNoise;
        private float _shakeTimer;

        private void Awake()
        {
            // Cinemachine'in Noise bileşenine erişiyoruz
            if (_thirdPersonCamera != null)
            {
                _cameraNoise = _thirdPersonCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }

        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerCameraShake += CameraShake;
        }

        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerCameraShake -= CameraShake;
        }

        private void Update()
        {
            // Sallanma süresini kontrol ediyoruz
            if (_shakeTimer > 0)
            {
                _shakeTimer -= Time.deltaTime;

                if (_shakeTimer <= 0f && _cameraNoise != null)
                {
                    // Sallanma süresi bittiğinde değerleri sıfırlıyoruz
                    _cameraNoise.m_AmplitudeGain = 0f;
                    _cameraNoise.m_FrequencyGain = 0f;
                }
            }
        }

        // Sallanma parametrelerini alacak şekilde değiştirildi
        private void CameraShake(float duration, float amplitude, float frequency)
        {
            if (_cameraNoise != null)
            {
                // Noise değerlerini ayarlıyoruz
                _cameraNoise.m_AmplitudeGain = amplitude;
                _cameraNoise.m_FrequencyGain = frequency;

                // Sallanma süresini başlatıyoruz
                _shakeTimer = duration;
            }
        }
    }
}
