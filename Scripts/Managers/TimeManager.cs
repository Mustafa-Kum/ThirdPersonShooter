using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Manager
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager instance;

        [SerializeField] private float _resumeTimeRate = 3;
        [SerializeField] private float _pauseTimeRate = 7;
        
        [Header("Cooldown UI")]
        [SerializeField] private Image _cooldownImage;
        
        // Public property to check if ability is on cooldown
        public bool IsOnCooldown => _isCooldownActive;
        
        private float _targetTimeScale = 1f;
        private float _timeAdjustRate;
        private float _currentCooldownTime;
        private float _maxCooldownTime;
        private bool _isCooldownActive;
        
        private void Awake()
        { 
            instance = this;
        }

        private void OnEnable()
        {
            EventManager.PlayerEvents.PlayerSlowMotion += SlowMotion;
        }
        
        private void OnDisable()
        {
            EventManager.PlayerEvents.PlayerSlowMotion -= SlowMotion;
        }

        private void Update()
        {
            if (Math.Abs(Time.timeScale - _targetTimeScale) > 0.01f)
            {
                float timeAdjustRate = Time.unscaledDeltaTime * _timeAdjustRate;
                
                Time.timeScale = Mathf.Lerp(Time.timeScale, _targetTimeScale, timeAdjustRate);
            }
            else
            {
                Time.timeScale = _targetTimeScale;
            }
            
            UpdateCooldownUI();
        }

        private void UpdateCooldownUI()
        {
            if (_isCooldownActive && _cooldownImage != null)
            {
                _currentCooldownTime -= Time.unscaledDeltaTime;
                
                float fillAmount = Mathf.Clamp01(_currentCooldownTime / _maxCooldownTime);
                _cooldownImage.fillAmount = fillAmount;
                
                if (_currentCooldownTime <= 0)
                {
                    _isCooldownActive = false;
                    _cooldownImage.fillAmount = 0;
                }
            }
        }
        
        public void PauseTime()
        {
            _timeAdjustRate = _pauseTimeRate;
            _targetTimeScale = 0f;
        }
        
        public void ResumeTime()
        {
            _timeAdjustRate = _resumeTimeRate;
            _targetTimeScale = 1f;
        }
        
        public void SlowMotion(float seconds)
        {
            // If already on cooldown, don't allow activation
            if (_isCooldownActive)
                return;
                
            StartCoroutine(SlowTimeCoroutine(seconds));
            
            if (_cooldownImage != null)
            {
                _maxCooldownTime = seconds;
                _currentCooldownTime = seconds;
                _isCooldownActive = true;
                _cooldownImage.fillAmount = 1f;
            }
        }

        private IEnumerator SlowTimeCoroutine(float seconds)
        {
            _targetTimeScale = 0.5f;
            Time.timeScale = _targetTimeScale;
            
            yield return new WaitForSecondsRealtime(seconds);
            
            ResumeTime();
        }
    }
}