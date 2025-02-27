using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Manager
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource[] _backgroundMusic;
        [SerializeField] private AudioSource[] _uiSFX;
        [SerializeField] private AudioSource[] _weaponFireSounds;
        [SerializeField] private AudioSource[] _weaponOutOfAmmoSounds;
        [SerializeField] private AudioSource _playerWalkSound;
        [SerializeField] private AudioSource _playerRunSound;
        [SerializeField] private AudioSource _playerRollSound;
        [SerializeField] private AudioSource _outOfAmmoVoice;
        [SerializeField] private AudioSource _bodyHitmarkerSound;
        [SerializeField] private AudioSource _headHitmarkerSound;
        [SerializeField] private CharacterController _playerController;
        [SerializeField] private PlayerWeaponSettingsSO _currentWeaponSO;

        private int _backgroundMusicIndex;
        private int _uiSFXIndex;
        
        private readonly Dictionary<WeaponType, AudioSource> _weaponTypeToFireSound = new Dictionary<WeaponType, AudioSource>();
        private readonly Dictionary<WeaponType, AudioSource> _weaponTypeToOutOfAmmoSound = new Dictionary<WeaponType, AudioSource>();

        private void Awake()
        {
            InitializeWeaponSounds();
            InitializeWeaponOutOfAmmoSounds();
        }

        private void OnEnable()
        {
            EventManager.AudioEvents.AudioWeaponScopeOutSoundAssign += PlayerWeaponScopeOutSound;
            EventManager.AudioEvents.AudioWeaponScopeInSoundAssign += PlayerWeaponScopeInSound;
            EventManager.AudioEvents.AudioPlayerFootStepWalkSound += PlayerWalkSound;
            EventManager.AudioEvents.AudioPlayerFootStepRunSound += PlayerRunSound;
            EventManager.AudioEvents.AudioEnemyMeleeSwooshSound += EnemyMeleeSwooshSound;
            EventManager.AudioEvents.AudioEnemyMeleeImpactSound += EnemyMeleeImpactSound;
            EventManager.AudioEvents.AudioWeaponOutOfAmmoSound += PlayWeaponOutOfAmmoSound;
            EventManager.AudioEvents.AudioBodyHitMarkerSound += BodyHitMarkerSound;
            EventManager.AudioEvents.AudioHeadHitMarkerSound += HeadHitMarkerSound;
            EventManager.AudioEvents.AudioWeaponFireSound += PlayWeaponFireSound;
            EventManager.AudioEvents.AudioPlayerRoll += PlayerRollSound;
            EventManager.AudioEvents.VoiceOutOfAmmo += OutOfAmmoVoice;
            EventManager.AudioEvents.AudioFadeOut += FadeOut;
            EventManager.AudioEvents.AudioFadeIn += FadeIn;
            EventManager.AudioEvents.AudioUISFX += PlayUiSFX;
        }

        private void OnDisable()
        {
            EventManager.AudioEvents.AudioWeaponScopeOutSoundAssign -= PlayerWeaponScopeOutSound;
            EventManager.AudioEvents.AudioWeaponScopeInSoundAssign -= PlayerWeaponScopeInSound;
            EventManager.AudioEvents.AudioPlayerFootStepWalkSound -= PlayerWalkSound;
            EventManager.AudioEvents.AudioPlayerFootStepRunSound -= PlayerRunSound;
            EventManager.AudioEvents.AudioEnemyMeleeSwooshSound -= EnemyMeleeSwooshSound;
            EventManager.AudioEvents.AudioEnemyMeleeImpactSound -= EnemyMeleeImpactSound;
            EventManager.AudioEvents.AudioWeaponOutOfAmmoSound -= PlayWeaponOutOfAmmoSound;
            EventManager.AudioEvents.AudioBodyHitMarkerSound -= BodyHitMarkerSound;
            EventManager.AudioEvents.AudioHeadHitMarkerSound -= HeadHitMarkerSound;
            EventManager.AudioEvents.AudioWeaponFireSound -= PlayWeaponFireSound;
            EventManager.AudioEvents.AudioPlayerRoll -= PlayerRollSound;
            EventManager.AudioEvents.VoiceOutOfAmmo -= OutOfAmmoVoice;
            EventManager.AudioEvents.AudioFadeOut -= FadeOut;
            EventManager.AudioEvents.AudioFadeIn -= FadeIn;
            EventManager.AudioEvents.AudioUISFX -= PlayUiSFX;
        }
        
        private void StopAllBackgroundMusic()
        {
            for(int i = 0; i < _backgroundMusic.Length; i++)
            {
                _backgroundMusic[i].Stop();
            }
        }
        
        private void PlayUiSFX(int index)
        {
            _uiSFXIndex = index;
            _uiSFX[index].Play();
        }
        
        private void PlayWeaponFireSound(WeaponType weaponType)
        {
            if (_weaponTypeToFireSound.TryGetValue(weaponType, out var audioSource))
            {
                AudioSource newAudioSource = LeanPool.Spawn(audioSource, transform);
                newAudioSource.Play();
                LeanPool.Despawn(newAudioSource.gameObject, newAudioSource.clip.length);
            }
            else
            {
                Debug.LogWarning($"No fire sound assigned for WeaponType {weaponType}");
            }
        }
        
        private void PlayWeaponOutOfAmmoSound(WeaponType weaponType)
        {
            if (_weaponTypeToOutOfAmmoSound.TryGetValue(weaponType, out var audioSource))
            {
                if (!audioSource.isPlaying) // Eğer ses çalmıyorsa
                {
                    audioSource.Play();
                }
            }
            else
            {
                Debug.LogWarning($"No out of ammo sound assigned for WeaponType {weaponType}");
            }
        }

        
        private void PlayerWalkSound(AudioSource audioSource, bool isPitch, float maxPitch, bool isLeftFoot)
        {
            if (_playerController.velocity.magnitude <= 0f)
                return;
            
            if (isLeftFoot)
                audioSource.panStereo = -0.15f;
            else
                audioSource.panStereo = 0.15f;
            
            float pitch = Random.Range(0.7f, maxPitch);
            audioSource.pitch = pitch;
            
            if (!_playerWalkSound.isPlaying)
                _playerWalkSound.Play();
        }
        
        
        private void PlayerWeaponScopeInSound(AudioSource audioSource)
        {
            audioSource.Play();
        }
        
        private void PlayerWeaponScopeOutSound(AudioSource audioSource)
        {
            audioSource.Play();
        }
        
        private void PlayerRollSound()
        {
            _playerRollSound.Play();
        }
        
        private void PlayerRunSound(AudioSource audioSource, bool isPitch, float maxPitch, bool isLeftFoot)
        {
            if (isLeftFoot)
                audioSource.panStereo = -0.15f;
            else
                audioSource.panStereo = 0.15f;
            
            float pitch = Random.Range(0.7f, maxPitch);
            audioSource.pitch = pitch;
            
            if (!_playerRunSound.isPlaying)
                _playerRunSound.Play();
        }

        private void BodyHitMarkerSound()
        {
            _bodyHitmarkerSound.Play();
        }

        private void HeadHitMarkerSound()
        {
            _headHitmarkerSound.Play();
        }

        private void OutOfAmmoVoice()
        {
            if (!_outOfAmmoVoice.isPlaying)
                _outOfAmmoVoice.Play();
        }
        
        private void EnemyMeleeSwooshSound(AudioSource audioSource, bool isPitch = false, float minPitch = 0.9f, float maxPitch = 1.1f)
        {
            if (audioSource == null)
                return;
            
            float pitch = Random.Range(minPitch, maxPitch);
            audioSource.pitch = pitch;
            audioSource.Play();
        }
        
        private void EnemyMeleeImpactSound(AudioSource audioSource, bool isPitch = false, float minPitch = 0.9f, float maxPitch = 1.1f)
        {
            if (audioSource == null)
                return;
            
            float pitch = Random.Range(minPitch, maxPitch);
            audioSource.pitch = pitch;
            audioSource.Play();
        }
        
        private void FadeOut(AudioSource audioSource, float fadeTime)
        {
            StartCoroutine(FadeOutAndStop(audioSource, fadeTime));
        }

        private void FadeIn(AudioSource audioSource, float fadeTime)
        {
            StartCoroutine(FadeInAndPlay(audioSource, fadeTime));
        }
        
        private void InitializeWeaponSounds()
        {
            _weaponTypeToFireSound.Clear();
            for (int i = 0; i < _weaponFireSounds.Length; i++)
            {
                if (i < System.Enum.GetValues(typeof(WeaponType)).Length)
                {
                    _weaponTypeToFireSound[(WeaponType)i] = _weaponFireSounds[i];
                }
            }
        }
        
        private void InitializeWeaponOutOfAmmoSounds()
        {
            _weaponTypeToOutOfAmmoSound.Clear();
            for (int i = 0; i < _weaponOutOfAmmoSounds.Length; i++)
            {
                if (i < System.Enum.GetValues(typeof(WeaponType)).Length)
                {
                    _weaponTypeToOutOfAmmoSound[(WeaponType)i] = _weaponOutOfAmmoSounds[i];
                }
            }
        }
        
        private IEnumerator FadeOutAndStop(AudioSource audioSource, float fadeTime)
        {
            float startVolume = audioSource.volume;

            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume;
        }

        private IEnumerator FadeInAndPlay(AudioSource audioSource, float fadeTime)
        {
            float startVolume = 0f;
            audioSource.volume = 0f;
            audioSource.Play();

            while (audioSource.volume < 1f)
            {
                audioSource.volume += Time.deltaTime / fadeTime;
                yield return null;
            }

            audioSource.volume = 1f; 
        }
    }
}