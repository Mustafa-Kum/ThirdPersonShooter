using System;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UILogic
{
    public class UISettings : MonoBehaviour
    {
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private float _sliderMultiplier = 25f;
        
        [Header("SFX Settings")]
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private TextMeshProUGUI _sfxSliderText;
        [SerializeField] private string _sfxParameterName;
        
        [Header("BGM Settings")]
        [SerializeField] private Slider _bgmSlider;
        [SerializeField] private TextMeshProUGUI _bgmSliderText;
        [SerializeField] private string _bgmParameterName;

        private void OnEnable()
        {
            EventManager.AudioEvents.AudioSettingsSave += LoadSettings;
        }

        private void OnDisable()
        {
            PlayerPrefs.SetFloat(_sfxParameterName, _sfxSlider.value);
            PlayerPrefs.SetFloat(_bgmParameterName, _bgmSlider.value);
            
            EventManager.AudioEvents.AudioSettingsSave -= LoadSettings;
        }

        public void LoadSettings()
        {
            _sfxSlider.value = PlayerPrefs.GetFloat(_sfxParameterName, 1);
            _bgmSlider.value = PlayerPrefs.GetFloat(_bgmParameterName, 1);
        }

        public void SFXSliderValue(float value)
        {
            _sfxSliderText.text = Mathf.RoundToInt(value * 100) + "%";
            float newValue = Mathf.Log10(value) * _sliderMultiplier;
            _audioMixer.SetFloat(_sfxParameterName, newValue);
        }
        
        public void BGMSliderValue(float value)
        {
            _bgmSliderText.text = Mathf.RoundToInt(value * 100) + "%";
            float newValue = Mathf.Log10(value) * _sliderMultiplier;
            _audioMixer.SetFloat(_bgmParameterName, newValue);
        }
    }
}