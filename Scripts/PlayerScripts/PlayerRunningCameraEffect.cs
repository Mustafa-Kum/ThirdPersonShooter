using System;
using Manager;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerRunningCameraEffect : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _bobFrequency = 3.5f; 
        [SerializeField] private float _bobAmplitude = 0.05f;
        [SerializeField] private float _returnSpeed = 5f;
        
        private Vector3 _originalPosition;
        private float _bobTimer;
        private bool _isRunning;

        private void Start()
        {
            // Kameranın başlangıç pozisyonunu kaydet
            if (_cameraTransform != null)
                _originalPosition = _cameraTransform.localPosition;
            else
                Debug.LogError("Kamera Transform'u atanmadı!");
        }

        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerRunningCameraEffectStart += RunningStarted;
            EventManager.PlayerEvents.PlayerRunningCameraEffectEnd += RunningEnded;
        }

        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerRunningCameraEffectStart -= RunningStarted;
            EventManager.PlayerEvents.PlayerRunningCameraEffectEnd -= RunningEnded;
        }
        
        private void RunningStarted()
        {
            // Bobbing zaman sayacını artır
            _bobTimer += Time.deltaTime * _bobFrequency;

            // Parabolik hareket (yukarı-aşağı)
            float phase = _bobTimer % 1; // 0 ile 1 arasında bir faz
            float parabola = 4 * phase * (1 - phase); // Parabolik hareket: 4x(1-x)
            float bobOffsetY = parabola * _bobAmplitude;

            // Kameranın yeni pozisyonunu ayarla
            Vector3 newPosition = _originalPosition;
            newPosition.y += bobOffsetY;

            _cameraTransform.localPosition = newPosition;
        }

        private void RunningEnded()
        {
            // Koşu bitince kamerayı orijinal pozisyonuna döndür
            _bobTimer = 0f; // İsterseniz sıfırlamayı kaldırabilirsiniz
            _cameraTransform.localPosition = Vector3.Lerp(
                _cameraTransform.localPosition,
                _originalPosition,
                Time.deltaTime * _returnSpeed
            );
        }
    }
}
