using System;
using Manager;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerWeaponScopeSound : MonoBehaviour
    {
        [SerializeField] private AudioSource _playerWeaponScopeInSound;
        [SerializeField] private AudioSource _playerWeaponScopeOutSound;

        private void OnEnable()
        {
            EventManager.AudioEvents.AudioWeaponScopeInSound += ScopeInSound;
            EventManager.AudioEvents.AudioWeaponScopeOutSound += ScopeOutSound;
        }
        
        private void OnDisable()
        {
            EventManager.AudioEvents.AudioWeaponScopeInSound -= ScopeInSound;
            EventManager.AudioEvents.AudioWeaponScopeOutSound -= ScopeOutSound;
        }

        private void ScopeInSound()
        {
            if (gameObject.activeSelf)
                EventManager.AudioEvents.AudioWeaponScopeInSoundAssign(_playerWeaponScopeInSound);
        }
        
        private void ScopeOutSound()
        {
            if (gameObject.activeSelf)
                EventManager.AudioEvents.AudioWeaponScopeOutSoundAssign(_playerWeaponScopeOutSound);
        }
    }
}