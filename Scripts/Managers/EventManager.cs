using System.Collections.Generic;
using Cinemachine;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;

namespace Manager
{
    public static partial class EventManager
    {
        public static class PlayerEvents
        {
            public static UnityAction<float, float, CinemachineVirtualCamera> PlayerFOVZoomRoutine;
            public static UnityAction<float, float, CinemachineVirtualCamera> PlayerCameraMoveY;
            public static UnityAction<float, float, float> PlayerWeaponFireZoomRoutine;
            public static UnityAction<float, float, float> PlayerCameraShake;
            public static UnityAction<float, float> PlayerWeaponRecoil;
            public static UnityAction<float, float> PlayerWeaponCameraShake;
            public static UnityAction<bool, Logic.HitArea> PlayerHitEnemyCrosshairFeedBack;
            public static UnityAction<float> PlayerSlowMotion;
            public static UnityAction<PlayerWeaponSettingsSO> PlayerWeaponPickUp;
            public static UnityAction<WeaponType> PlayerWeaponReload;
            public static UnityAction<Transform> PlayerScaleToZeroForCar;
            public static UnityAction<bool> PlayerCanTakeDamage;
            public static UnityAction<bool> PlayerControlBool;
            public static UnityAction<bool> PlayerWeaponCanFire;
            public static UnityAction<bool> PlayerScopedAnimation;
            public static UnityAction<bool> PlayerRollingStart;
            public static UnityAction<bool> PlayerRollingEnd;
            public static UnityAction<bool> PlayerCanScopeToggle;
            public static UnityAction<int> PlayerWeaponSwap;
            public static UnityAction PlayerMovement;
            public static UnityAction PlayerFire;
            public static UnityAction PlayerAim;
            public static UnityAction PlayerScopedAim;
            public static UnityAction PlayerInteractable;
            public static UnityAction PlayerWeaponDrop;
            public static UnityAction PlayerAmmoPickedUp;
            public static UnityAction PlayerHealthPickedUp;
            public static UnityAction PlayerScaleToDefault;
            public static UnityAction PlayerColliderFalse;
            public static UnityAction PlayerColliderTrue;
            public static UnityAction PlayerAimLaserTrue;
            public static UnityAction PlayerAimLaserFalse;
            public static UnityAction PlayerLeftFootStepParticle;
            public static UnityAction PlayerRightFootStepParticle;
            public static UnityAction PlayerRollParticle;
            public static UnityAction PlayerRunningCameraEffectStart;
            public static UnityAction PlayerRunningCameraEffectEnd;
            public static UnityAction PlayerRollingCameraEffect;
            public static UnityAction PlayerFallingToGround;
            public static UnityAction PlayerGetUpFromGround;
        }
        
        public static class GameEvents
        {
            public static UnityAction GameStart;
            public static UnityAction GameOver;
            public static UnityAction LevelCompleted;
            public static UnityAction LevelGeneration;
            public static UnityAction MissionStart;
        }
        
        public static class UIEvents
        {
            public static UnityAction<List<PlayerWeaponSettingsSO>, PlayerWeaponSettingsSO> UIWeaponUpdate;
            public static UnityAction<PlayerWeaponIndexSO> UIWeaponAlpha;
            public static UnityAction UIMissionToolTipSwitch;
            public static UnityAction UIFadeIn;
            public static UnityAction UIFadeOut;
        }

        public static class AudioEvents
        {
            public static UnityAction<AudioSource, bool, float, float> AudioEnemyMeleeSwooshSound;
            public static UnityAction<AudioSource, bool, float, float> AudioEnemyMeleeImpactSound;
            public static UnityAction<AudioSource, bool, float, bool> AudioPlayerFootStepWalkSound;
            public static UnityAction<AudioSource, bool, float, bool> AudioPlayerFootStepRunSound;
            public static UnityAction<AudioSource, float> AudioFadeOut;
            public static UnityAction<AudioSource, float> AudioFadeIn;
            public static UnityAction<AudioSource> AudioWeaponScopeInSoundAssign;
            public static UnityAction<AudioSource> AudioWeaponScopeOutSoundAssign;
            public static UnityAction<WeaponType> AudioWeaponFireSound;
            public static UnityAction<WeaponType> AudioWeaponOutOfAmmoSound;
            public static UnityAction<int> AudioUISFX;
            public static UnityAction AudioWeaponScopeInSound;
            public static UnityAction AudioWeaponScopeOutSound;
            public static UnityAction AudioPlayerRoll;
            public static UnityAction AudioSettingsSave;
            public static UnityAction VoiceOutOfAmmo;
            public static UnityAction AudioBodyHitMarkerSound;
            public static UnityAction AudioHeadHitMarkerSound;
            public static UnityAction AudioKillSound;
        }

        public static class EnemySpawnEvents
        {
            public static UnityAction EnemySpawn;
            public static UnityAction ResetEnemySpawnWave;
            public static UnityAction<GameObject> EnemyDied;
        }

        public static class MapEvents
        {
            public static UnityAction ColumnSpawn;
        }
    }
}