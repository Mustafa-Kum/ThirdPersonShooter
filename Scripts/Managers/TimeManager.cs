using System;
using System.Collections;
using UnityEngine;

namespace Manager
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager instance;

        [SerializeField] private float _resumeTimeRate = 3;
        [SerializeField] private float _pauseTimeRate = 7;
        
        private float _targetTimeScale = 1f;
        private float _timeAdjustRate;
        
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
            StartCoroutine(SlowTimeCoroutine(seconds));
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